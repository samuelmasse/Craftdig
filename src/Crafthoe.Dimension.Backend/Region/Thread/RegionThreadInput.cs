namespace Craftdig.Dimension.Backend;

public record struct RegionThreadInput(Vector3i Sloc, RegionThreadInputType Type, ChunkBlocks Blocks, int SectionZ);
