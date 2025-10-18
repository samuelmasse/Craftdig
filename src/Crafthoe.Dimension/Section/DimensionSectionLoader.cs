namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionLoader(
    DimensionBlockProgram blockProgram,
    DimensionMetrics metrics,
    DimensionSectionMesher sectionMesher,
    DimensionSectionMeshTransferer meshTransferer)
{
    public void Load(EntMut section)
    {
        metrics.SectionMetric.Start();

        sectionMesher.Render(section.Sloc());

        meshTransferer.Transfer(
            blockProgram,
            sectionMesher.Vertices,
            ref section.TerrainMesh());

        sectionMesher.Reset();

        section.Chunk().Unrendered().Remove(section.Sloc().Z);
        if (!section.Chunk().Rendered().ContainsKey(section.Sloc().Z))
            section.Chunk().Rendered().Add(section.Sloc().Z, section.Sloc().Z);

        metrics.SectionMetric.End();
    }
}
