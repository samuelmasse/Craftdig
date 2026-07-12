namespace Craftdig.Dimension.Backend;

public readonly record struct ChunkThreadInput(ChunkBlocks Blocks, Vec2i Cloc, bool Noop);
