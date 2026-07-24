namespace Craftdig.Protocol;

public delegate bool RawCommandValidator(ReadOnlySpan<byte> body);
