namespace Crafthoe.Dimension;

public record struct ChunkThreadInput(Memory<Ent> Blocks, Vector2i Cloc, bool Noop);
