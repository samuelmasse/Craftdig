namespace Craftdig;

[Dimension]
public class DimensionChunkFrontendUnloader(
    DimensionSectionMeshTransferer meshTransferer,
    DimensionSections sections,
    DimensionChunkSortedLists chunkSortedLists,
    DimensionChunkRenderDescheduler chunkRenderDescheduler)
{
    public void Unload(EntMutIdx chunk)
    {
        foreach (ref var section in chunk.Sections.Span)
        {
            if (section != default)
                section.TerrainMesh = meshTransferer.Free(section.TerrainMesh);

            section.Dispose();
        }

        if (!chunk.Sections.IsEmpty)
            sections.ReturnSections(chunk.Sections);

        chunkSortedLists.Return(chunk.Unrendered);
        chunkSortedLists.Return(chunk.Rendered);

        chunkRenderDescheduler.Remove(chunk.Cloc);
    }
}
