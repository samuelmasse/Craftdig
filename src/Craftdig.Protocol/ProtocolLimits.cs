namespace Craftdig;

public static class ProtocolLimits
{
    public const int SegmentSize = short.MaxValue;
    public const int FrameHeaderSize = sizeof(ushort) + sizeof(int);
    public const int MaxMessageSize = SegmentSize - FrameHeaderSize;
    public const int MaxClientMessageSize = 4096;

    public const int MaxIdentityTicketSize = 3072;
    public const int MaxPresencePlayers = 1000;
    public const int MaxPresenceActiveRounds = 2;
    public const int MaxPresenceRoundChunks = MaxPresencePlayers;
    public const int MaxPresenceRoundRecordsPerChunk = 584;
    public const int MaxPresenceProofRecordsPerBatch = 340;
}
