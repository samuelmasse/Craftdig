namespace Crafthoe.Dimension.Backend;

public record struct ChunkThreadInput(Memory<Ent> Blocks, Vector2i Cloc, bool Noop);
