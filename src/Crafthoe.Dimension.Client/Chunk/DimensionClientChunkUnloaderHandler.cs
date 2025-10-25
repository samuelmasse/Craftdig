namespace Crafthoe.Dimension;

[Dimension]
public class DimensionClientChunkUnloaderHandler(
    DimensionSectionMeshTransferer meshTransferer,
    DimensionSections sections,
    DimensionChunkSortedLists chunkSortedLists,
    DimensionChunkRenderDescheduler chunkRenderDescheduler)
{
    public void Handler(EntMut chunk)
    {
        foreach (var section in chunk.GetSections())
        {
            if (section != default)
                meshTransferer.Free(ref section.TerrainMesh());

            section.Dispose();
        }

        if (!chunk.Sections().IsEmpty)
            sections.ReturnSections(chunk.Sections());

        chunkSortedLists.Return(chunk.Unrendered());
        chunkSortedLists.Return(chunk.Rendered());

        chunkRenderDescheduler.Remove(chunk.Cloc());
    }
}
