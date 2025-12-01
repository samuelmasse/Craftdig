namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionChunkBackendUnloader(
    DimensionBlocksRaw blocksRaw,
    DimensionRegionThreadWorkQueue regionThreadWorkQueue,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Unload(EntMut ent)
    {
        var cloc = ent.Cloc();

        for (int sz = 0; sz < SectionHeight; sz++)
            regionInvalidation.Drain(new(cloc, sz));

        if (blocksRaw.TryGetChunkBlocks(cloc, out var blocks))
            regionThreadWorkQueue.Enqeue(new(new(cloc, 0), RegionThreadInputType.DisposeChunk, blocks, 0));
    }
}
