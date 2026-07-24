namespace Craftdig.Protocol;

public sealed class ValidatedIdentityTicket
{
    private readonly byte[] rawTicket;

    public ValidatedIdentityTicket(
        ReadOnlySpan<byte> rawTicket,
        Guid playerId,
        string username,
        SessionId sessionId,
        ServerContext serverContext,
        P256PublicKey publicKey,
        string keyId,
        Guid ticketId,
        DateTimeOffset issuedAt,
        DateTimeOffset notBefore,
        DateTimeOffset expiresAt)
    {
        if (!PlayerIdentityCommandCodec.IsCompactJwt(rawTicket))
            throw new ArgumentException("The raw ticket is not a bounded compact JWT.", nameof(rawTicket));
        if (playerId == Guid.Empty)
            throw new ArgumentException("The player ID cannot be empty.", nameof(playerId));
        if (ticketId == Guid.Empty)
            throw new ArgumentException("The ticket ID cannot be empty.", nameof(ticketId));
        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("The username cannot be empty.", nameof(username));
        if (string.IsNullOrEmpty(keyId))
            throw new ArgumentException("The signing key ID cannot be empty.", nameof(keyId));
        if (notBefore < issuedAt || expiresAt <= notBefore || expiresAt - issuedAt > TimeSpan.FromMinutes(15))
            throw new ArgumentException("The ticket timestamps do not describe a valid 15-minute lifetime.", nameof(expiresAt));

        this.rawTicket = rawTicket.ToArray();
        TicketHash = Hash256.Compute(this.rawTicket);
        PlayerId = playerId;
        Username = username;
        SessionId = sessionId;
        ServerContext = serverContext;
        PublicKey = publicKey;
        KeyId = keyId;
        TicketId = ticketId;
        IssuedAt = issuedAt;
        NotBefore = notBefore;
        ExpiresAt = expiresAt;
    }

    public readonly Hash256 TicketHash;
    public readonly Guid PlayerId;
    public readonly string Username;
    public readonly SessionId SessionId;
    public readonly ServerContext ServerContext;
    public readonly P256PublicKey PublicKey;
    public readonly string KeyId;
    public readonly Guid TicketId;
    public readonly DateTimeOffset IssuedAt;
    public readonly DateTimeOffset NotBefore;
    public readonly DateTimeOffset ExpiresAt;

    public ReadOnlySpan<byte> RawTicket => rawTicket;
    public int RawTicketSize => rawTicket.Length;

    public bool TryCopyRawTicket(Span<byte> destination)
    {
        if (destination.Length < rawTicket.Length)
            return false;

        rawTicket.CopyTo(destination);
        return true;
    }
}
