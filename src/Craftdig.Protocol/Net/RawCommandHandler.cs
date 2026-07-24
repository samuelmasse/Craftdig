namespace Craftdig.Protocol;

public delegate void RawCommandHandler(NetSocket socket, ReadOnlySpan<byte> body);
