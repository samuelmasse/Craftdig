namespace Craftdig.Menus.Multiplayer;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.IdentityModel.Tokens;

[Module]
public class ModuleIdentityTicketClient(
    Log log,
    ModuleMultiplayerCredentials credentials,
    ModuleIdentityTrust identityTrust,
    DevIdentityConfig dev)
{
    private const string TicketUrl = "https://craftdig.io/api/GetToken";
    private const int MaxResponseBytes = 8 * 1024;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(8);
    private readonly HttpClient http = new();

    public ValidatedIdentityTicket Request(PlayerIdentitySession session, CancellationToken cancellationToken)
    {
        if (!session.IdentityEnabled || session.ServerContext == null)
        {
            log.Warn("Refusing to request an Identity ticket for a development or unbound session");
            throw new InvalidOperationException("Development connections do not request Identity tickets.");
        }

        if (dev.Enabled)
            return IssueDevTicket(session, cancellationToken);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(RequestTimeout);

        byte[] rawTicket = FetchRawTicket(session, timeout.Token);
        return ValidateIssuedTicket(session, rawTicket, timeout.Token);
    }

    private ValidatedIdentityTicket IssueDevTicket(PlayerIdentitySession session, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(dev.Name))
            throw new InvalidOperationException("Dev identity is enabled but no dev identity name is configured.");

        var key = DevIdentityKey.LoadOrCreate(DevIdentityKey.DefaultPath);
        byte[] rawTicket = new DevIdentityTicketIssuer(key).Issue(
            DevIdentityKey.PlayerId(dev.Name),
            dev.Name,
            session.SessionId,
            session.ServerContext!,
            session.PublicKey,
            DateTimeOffset.UtcNow.AddSeconds(-1),
            TimeSpan.FromMinutes(10));
        log.Warn("Signing a development Identity ticket locally for dev player {0}; craftdig.io is bypassed", dev.Name);
        return ValidateIssuedTicket(session, rawTicket, cancellationToken);
    }

    private byte[] FetchRawTicket(PlayerIdentitySession session, CancellationToken cancellationToken)
    {
        using var request = BuildTicketRequest(session, cancellationToken);
        using var response = http.Send(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            log.Warn("Identity rejected the multiplayer ticket request with HTTP {0}", (int)response.StatusCode);
            throw new InvalidDataException($"Craftdig Identity rejected the multiplayer ticket request ({(int)response.StatusCode}).");
        }

        if (response.Content.Headers.ContentLength > MaxResponseBytes)
        {
            log.Warn("Rejecting oversized Identity ticket response; declared bytes: {0}", response.Content.Headers.ContentLength ?? -1);
            throw new InvalidDataException("Craftdig Identity returned an oversized response.");
        }

        response.Content.LoadIntoBufferAsync(MaxResponseBytes, cancellationToken).GetAwaiter().GetResult();
        byte[] responseBytes = response.Content.ReadAsByteArrayAsync(cancellationToken).GetAwaiter().GetResult();
        return ReadJwtEnvelope(responseBytes);
    }

    private HttpRequestMessage BuildTicketRequest(PlayerIdentitySession session, CancellationToken cancellationToken)
    {
        byte[] xBytes = new byte[Hash256.Size];
        byte[] yBytes = new byte[Hash256.Size];
        session.PublicKey.X.TryWrite(xBytes);
        session.PublicKey.Y.TryWrite(yBytes);

        var request = new HttpRequestMessage(HttpMethod.Post, TicketUrl)
        {
            Content = JsonContent.Create(new
            {
                version = 1,
                server = new
                {
                    host = session.ServerContext!.Host,
                    port = session.ServerContext.Port,
                },
                sessionId = session.SessionId.ToString(),
                publicKey = new
                {
                    kty = "EC",
                    crv = "P-256",
                    x = Base64UrlEncoder.Encode(xBytes),
                    y = Base64UrlEncoder.Encode(yBytes),
                },
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            credentials.GetFreshToken(cancellationToken));
        return request;
    }

    private byte[] ReadJwtEnvelope(byte[] responseBytes)
    {
        using var document = JsonDocument.Parse(responseBytes, new() { MaxDepth = 3 });
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object || root.EnumerateObject().Count() != 1 ||
            !root.TryGetProperty("jwt", out var jwtProperty) || jwtProperty.ValueKind != JsonValueKind.String ||
            jwtProperty.GetString() is not { } jwt)
        {
            log.Warn("Rejecting Identity ticket response because its JSON envelope is not canonical");
            throw new InvalidDataException("Craftdig Identity returned an invalid multiplayer ticket response.");
        }

        return Encoding.ASCII.GetBytes(jwt);
    }

    private ValidatedIdentityTicket ValidateIssuedTicket(
        PlayerIdentitySession session,
        byte[] rawTicket,
        CancellationToken cancellationToken)
    {
        var ticket = identityTrust.Validate(rawTicket, session.ServerContext!, out var failure, cancellationToken);
        if (ticket == null)
        {
            log.Warn("Rejecting Identity ticket; validation stage: {0}", failure);
            throw new InvalidDataException($"Craftdig Identity returned an invalid multiplayer ticket ({failure}).");
        }

        if (ticket.SessionId != session.SessionId)
        {
            log.Warn("Rejecting Identity ticket because its session ID does not match the requested connection session");
            throw new InvalidDataException("Craftdig Identity returned a ticket for another connection key or session.");
        }

        if (ticket.PublicKey != session.PublicKey)
        {
            log.Warn("Rejecting Identity ticket because its proof-of-possession key does not match the connection key");
            throw new InvalidDataException("Craftdig Identity returned a ticket for another connection key or session.");
        }

        log.Info(
            "Identity ticket acquisition succeeded for player {0}; issued at {1}; expires at {2}",
            ticket.PlayerId, ticket.IssuedAt, ticket.ExpiresAt);
        return ticket;
    }
}
