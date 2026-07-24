namespace Craftdig.Protocol;

public delegate bool CommandDecoder<V>(ReadOnlySpan<byte> body, out V value);
