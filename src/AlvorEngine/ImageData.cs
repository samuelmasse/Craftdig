namespace AlvorEngine;

public record struct ImageData(Vector2i Size, ReadOnlyMemory<(byte, byte, byte, byte)> Pixels);
