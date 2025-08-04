namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkUnloader(
    DimensionChunks chunks,
    DimensionChunkIndex chunkIndex,
    DimensionMeshTransferer meshTransferer,
    DimensionSections sections,
    DimensionBlocks blocks,
    DimensionChunkRenderDescheduler chunkRenderDescheduler)
{
    public void Unload(Vector2i cloc)
    {
        if (!chunks.TryGet(cloc, out var chunk))
            return;

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
            blocks.ReturnBlocks(chunk.Blocks());

        chunks.Free(cloc);
    }
}
