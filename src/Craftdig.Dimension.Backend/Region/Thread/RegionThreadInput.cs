namespace Craftdig;

public record struct RegionThreadInput(Vec3i Sloc, RegionThreadInputType Type, ChunkBlocks Blocks, int SectionZ);
