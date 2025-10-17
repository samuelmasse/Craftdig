namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkUnloader(
    DimensionChunks chunks,
    DimensionChunkBag chunkIndex,
    DimensionMeshTransferer meshTransferer,
    DimensionSections sections,
    DimensionBlocksRaw blocksRaw,
    DimensionChunkRenderDescheduler chunkRenderDescheduler,
    DimensionRegionInvalidation regionInvalidation)
{
    public void Unload(Vector2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return;

        for (int sz = 0; sz < SectionHeight; sz++)
            regionInvalidation.Drain(new(cloc, sz));

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
        if (!chunk.Blocks().IsEmpty)
            blocksRaw.Return(chunk.Blocks());

        chunks.Free(cloc);
    }
}
