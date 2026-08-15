namespace Craftdig;

public static class PresenceProofDigest
{
    private static ReadOnlySpan<byte> DomainSeparator => "Craftdig presence proof v1\0"u8;

    public static Hash256 Compute(Hash256 serverContextHash, Hash256 roundHash, Hash256 ticketHash) =>
        DomainDigest.Compute(DomainSeparator, serverContextHash, roundHash, ticketHash);
}
