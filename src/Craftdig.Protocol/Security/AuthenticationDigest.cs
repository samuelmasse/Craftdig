namespace Craftdig.Protocol;

public static class AuthenticationDigest
{
    private static ReadOnlySpan<byte> DomainSeparator => "Craftdig authentication v1\0"u8;

    public static Hash256 Compute(Hash256 serverContextHash, Nonce256 serverNonce, Hash256 ticketHash) =>
        DomainDigest.Compute(DomainSeparator, serverContextHash, serverNonce, ticketHash);
}
