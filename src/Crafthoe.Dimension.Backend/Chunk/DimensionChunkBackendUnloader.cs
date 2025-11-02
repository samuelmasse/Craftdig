namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionChunkBackendUnloader(
    DimensionRegionThreadWorkQueue regionThreadWorkQueue,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Unload(EntMut ent)
    {
        var cloc = ent.Cloc();

        for (int sz = 0; sz < SectionHeight; sz++)
            regionInvalidation.Drain(new(cloc, sz));

        if (!ent.Blocks().IsEmpty)
            regionThreadWorkQueue.Enqeue(new(new(cloc, 0), RegionThreadInputType.DisposeChunk, ent.Blocks()));
    }
}
