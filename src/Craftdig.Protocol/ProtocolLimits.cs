namespace Craftdig.Protocol;

public static class ProtocolLimits
{
    public const int SegmentSize = short.MaxValue;
    public const int FrameHeaderSize = sizeof(ushort) + sizeof(int);
    public const int MaxMessageSize = SegmentSize - FrameHeaderSize;
    public const int MaxClientMessageSize = 4096;
}
