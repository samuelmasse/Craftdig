namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkUnloader(
    DimensionChunks chunks,
    DimensionChunkBag chunkIndex,
    DimensionSectionMeshTransferer meshTransferer,
    DimensionSections sections,
    DimensionChunkRenderDescheduler chunkRenderDescheduler,
    DimensionChunkSortedLists chunkSortedLists,
    DimensionRegionThreadWorkQueue regionThreadWorkQueue,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Unload(Vector2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return;

        for (int sz = 0; sz < SectionHeight; sz++)
            regionInvalidation.Drain(new(cloc, sz));

        if (!chunk.Blocks().IsEmpty)
            regionThreadWorkQueue.Enqeue(new(new(cloc, 0), RegionThreadInputType.DisposeChunk, chunk.Blocks()));

        foreach (var section in chunk.GetSections())
        {
            if (section != default)
                meshTransferer.Free(ref section.TerrainMesh());

            section.Dispose();
        }

        chunkIndex.Remove(chunk);
        chunkRenderDescheduler.Remove(cloc);

        if (!chunk.Sections().IsEmpty)
            sections.ReturnSections(chunk.Sections());

        chunkSortedLists.Return(chunk.Unrendered());
        chunkSortedLists.Return(chunk.Rendered());

        chunks.Free(cloc);
    }
}
