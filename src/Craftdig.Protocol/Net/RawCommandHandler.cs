namespace Craftdig;

public delegate void RawCommandHandler(NetSocket socket, ReadOnlySpan<byte> body);
