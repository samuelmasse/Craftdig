namespace Craftdig.Dimension.Backend;

public readonly record struct ChunkThreadOutput(ChunkBlocks Blocks, Vec2i Cloc, bool Noop, ChunkLight Light);
