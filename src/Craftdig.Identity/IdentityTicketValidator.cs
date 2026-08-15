namespace Craftdig;

public class IdentityTicketValidator(IdentityJwksCache jwks, Func<ServerContext, bool>? allowedContexts = null)
{
    private readonly JwtSecurityTokenHandler tokens = new()
    {
        MapInboundClaims = false,
        MaximumTokenSizeInBytes = ProtocolLimits.MaxIdentityTicketSize,
    };

    public ValidatedIdentityTicket? Validate(
        ReadOnlySpan<byte> rawTicket,
        out IdentityTicketFailure failure,
        CancellationToken cancellationToken = default) =>
        ValidateCore(rawTicket, null, out failure, cancellationToken);

    public ValidatedIdentityTicket? Validate(
        ReadOnlySpan<byte> rawTicket,
        ServerContext expectedContext,
        out IdentityTicketFailure failure,
        CancellationToken cancellationToken = default) =>
        ValidateCore(rawTicket, expectedContext, out failure, cancellationToken);

    private ValidatedIdentityTicket? ValidateCore(
        ReadOnlySpan<byte> rawTicket,
        ServerContext? expectedContext,
        out IdentityTicketFailure failure,
        CancellationToken cancellationToken)
    {
        failure = IdentityTicketFailure.Invalid;
        if (!PlayerIdentityCommandCodec.IsCompactJwt(rawTicket))
        {
            return null;
        }

        byte[] ticketBytes = rawTicket.ToArray();
        try
        {
            if (!TryParse(ticketBytes, expectedContext, out var parsed, out failure))
                return null;

            var signingKey = jwks.GetKey(parsed.KeyId, cancellationToken);
            if (signingKey == null)
            {
                failure = IdentityTicketFailure.SigningKeyUnavailable;
                return null;
            }

            if (!ValidateSignature(ticketBytes, signingKey))
            {
                failure = IdentityTicketFailure.Signature;
                return null;
            }

            return new(
                ticketBytes,
                parsed.PlayerId,
                parsed.Username,
                parsed.SessionId,
                parsed.ServerContext,
                parsed.PublicKey,
                parsed.KeyId,
                parsed.TicketId,
                parsed.IssuedAt,
                parsed.NotBefore,
                parsed.ExpiresAt);
        }
        catch (Exception e) when (e is ArgumentException or JsonException or FormatException or CryptographicException)
        {
            failure = IdentityTicketFailure.Invalid;
            return null;
        }
    }

    private bool TryParse(
        ReadOnlySpan<byte> rawTicket,
        ServerContext? expectedContext,
        [NotNullWhen(true)] out ParsedTicket? parsed,
        out IdentityTicketFailure failure)
    {
        parsed = null;
        failure = IdentityTicketFailure.Invalid;
        if (!TryDecodeSegments(rawTicket, out var headerBytes, out var payloadBytes))
            return false;

        using var headerDocument = JsonDocument.Parse(headerBytes, StrictJson.DocumentOptions);
        using var payloadDocument = JsonDocument.Parse(payloadBytes, StrictJson.DocumentOptions);
        var payload = payloadDocument.RootElement;

        if (!TryReadHeader(headerDocument.RootElement, out string? keyId))
            return false;

        if (!StrictJson.HasPropertyCount(payload, 12) ||
            !StrictJson.TryGetString(payload, "iss", out string? issuer) || issuer != IdentityTicketFormat.Issuer ||
            !StrictJson.TryGetString(payload, "aud", out string? audience) || audience != IdentityTicketFormat.Audience ||
            !StrictJson.TryGetCanonicalInt64(payload, "ver", out long version) || version != IdentityTicketFormat.Version)
            return false;

        if (!StrictJson.TryGetCanonicalUuid(payload, "sub", out Guid playerId) ||
            !StrictJson.TryGetString(payload, "username", out string? username) || !IdentityTicketFormat.IsUsername(username) ||
            !StrictJson.TryGetCanonicalUuid(payload, "sid", out Guid sessionGuid) ||
            !SessionId.TryFromGuid(sessionGuid, out var sessionId) ||
            !StrictJson.TryGetCanonicalUuid(payload, "jti", out Guid ticketId))
            return false;

        if (!StrictJson.TryGetCanonicalInt64(payload, "iat", out long issuedAtSeconds) ||
            !StrictJson.TryGetCanonicalInt64(payload, "nbf", out long notBeforeSeconds) ||
            !StrictJson.TryGetCanonicalInt64(payload, "exp", out long expiresAtSeconds) ||
            !StrictJson.TryGetUnique(payload, "server", out var server) ||
            !TryReadServerContext(server, out var serverContext) ||
            !StrictJson.TryGetUnique(payload, "cnf", out var confirmation) ||
            !TryReadPublicKey(confirmation, out var publicKey))
            return false;

        if (expectedContext != null ? serverContext != expectedContext : allowedContexts?.Invoke(serverContext) != true)
        {
            failure = IdentityTicketFailure.ContextNotAllowed;
            return false;
        }

        if (!TryReadLifetime(
                issuedAtSeconds,
                notBeforeSeconds,
                expiresAtSeconds,
                out var issuedAt,
                out var notBefore,
                out var expiresAt))
        {
            failure = IdentityTicketFailure.Lifetime;
            return false;
        }

        failure = IdentityTicketFailure.None;
        parsed = new(
            keyId,
            playerId,
            username,
            sessionId,
            ticketId,
            serverContext,
            publicKey,
            issuedAt,
            notBefore,
            expiresAt);
        return true;
    }

    private static bool TryReadLifetime(
        long issuedAtSeconds,
        long notBeforeSeconds,
        long expiresAtSeconds,
        out DateTimeOffset issuedAt,
        out DateTimeOffset notBefore,
        out DateTimeOffset expiresAt)
    {
        notBefore = default;
        expiresAt = default;
        try
        {
            issuedAt = DateTimeOffset.FromUnixTimeSeconds(issuedAtSeconds);
            notBefore = DateTimeOffset.FromUnixTimeSeconds(notBeforeSeconds);
            expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresAtSeconds);
        }
        catch (ArgumentOutOfRangeException)
        {
            issuedAt = default;
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var skew = IdentityTicketFormat.ClockSkew;
        return issuedAt <= now + skew && notBefore >= issuedAt && notBefore <= now + skew &&
            expiresAt > notBefore && expiresAt >= now - skew && expiresAt > issuedAt &&
            expiresAt - issuedAt <= IdentityTicketFormat.MaximumLifetime;
    }

    private bool ValidateSignature(ReadOnlySpan<byte> ticketBytes, SecurityKey signingKey)
    {
        try
        {
            tokens.ValidateToken(Encoding.ASCII.GetString(ticketBytes), new()
            {
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                TryAllIssuerSigningKeys = false,
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                ValidateIssuer = true,
                ValidIssuer = IdentityTicketFormat.Issuer,
                ValidateAudience = true,
                ValidAudience = IdentityTicketFormat.Audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = IdentityTicketFormat.ClockSkew,
                ValidTypes = [IdentityTicketFormat.TicketType],
            }, out var validatedToken);

            return validatedToken is JwtSecurityToken token &&
                token.Header.Alg == SecurityAlgorithms.RsaSha256 &&
                token.Header.Typ == IdentityTicketFormat.TicketType;
        }
        catch (Exception e) when (e is SecurityTokenException or ArgumentException)
        {
            return false;
        }
    }

    private static bool TryReadServerContext(JsonElement value, [NotNullWhen(true)] out ServerContext? context)
    {
        context = null;
        return StrictJson.HasPropertyCount(value, 2) &&
            StrictJson.TryGetString(value, "host", out string? host) &&
            StrictJson.TryGetCanonicalInt64(value, "port", out long port) &&
            port is >= 1 and <= ushort.MaxValue &&
            ServerContext.TryParseCanonical(host, (int)port, out context);
    }

    private static bool TryReadPublicKey(JsonElement confirmation, out P256PublicKey publicKey)
    {
        publicKey = default;
        if (!StrictJson.HasPropertyCount(confirmation, 1) || !StrictJson.TryGetUnique(confirmation, "jwk", out var jwk) ||
            !StrictJson.HasPropertyCount(jwk, 4) ||
            !StrictJson.TryGetString(jwk, "kty", out string? keyType) || keyType != "EC" ||
            !StrictJson.TryGetString(jwk, "crv", out string? curve) || curve != "P-256" ||
            !StrictJson.TryGetString(jwk, "x", out string? xText) || !TryDecodeCoordinate(xText, out var x) ||
            !StrictJson.TryGetString(jwk, "y", out string? yText) || !TryDecodeCoordinate(yText, out var y))
            return false;

        return P256PublicKey.TryCreate(x, y, out publicKey);
    }

    private static bool TryDecodeCoordinate(string value, [NotNullWhen(true)] out byte[]? coordinate)
    {
        coordinate = null;
        return value.Length == 43 &&
            StrictJson.TryDecodeCanonicalBase64Url(value, out coordinate) &&
            coordinate.Length == P256PublicKey.CoordinateSize;
    }

    private static bool TryDecodeSegments(
        ReadOnlySpan<byte> rawTicket,
        [NotNullWhen(true)] out byte[]? headerBytes,
        [NotNullWhen(true)] out byte[]? payloadBytes)
    {
        headerBytes = null;
        payloadBytes = null;
        int firstDot = rawTicket.IndexOf((byte)'.');
        int secondDotOffset = firstDot < 1 ? -1 : rawTicket[(firstDot + 1)..].IndexOf((byte)'.');
        if (secondDotOffset < 1)
            return false;

        int secondDot = firstDot + 1 + secondDotOffset;
        return TryDecodeSegment(rawTicket[..firstDot], out headerBytes) &&
            TryDecodeSegment(rawTicket[(firstDot + 1)..secondDot], out payloadBytes) &&
            TryDecodeSegment(rawTicket[(secondDot + 1)..], out var signatureBytes) &&
            signatureBytes.Length is >= 256 and <= 512;
    }

    private static bool TryReadHeader(JsonElement header, [NotNullWhen(true)] out string? keyId)
    {
        keyId = null;
        return StrictJson.HasPropertyCount(header, 3) &&
            StrictJson.TryGetString(header, "alg", out string? algorithm) && algorithm == SecurityAlgorithms.RsaSha256 &&
            StrictJson.TryGetString(header, "typ", out string? ticketType) && ticketType == IdentityTicketFormat.TicketType &&
            StrictJson.TryGetString(header, "kid", out keyId) && IdentityTicketFormat.IsKeyId(keyId);
    }

    private static bool TryDecodeSegment(ReadOnlySpan<byte> encoded, [NotNullWhen(true)] out byte[]? decoded)
    {
        decoded = null;
        try
        {
            string text = Encoding.ASCII.GetString(encoded);
            return StrictJson.TryDecodeCanonicalBase64Url(text, out decoded);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private sealed record ParsedTicket(
        string KeyId,
        Guid PlayerId,
        string Username,
        SessionId SessionId,
        Guid TicketId,
        ServerContext ServerContext,
        P256PublicKey PublicKey,
        DateTimeOffset IssuedAt,
        DateTimeOffset NotBefore,
        DateTimeOffset ExpiresAt);
}
