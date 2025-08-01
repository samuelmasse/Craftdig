namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionLoader(
    RootPositionColorTextureProgram3D positionColorTextureProgram3D,
    DimensionMetrics metrics,
    DimensionChunks chunks,
    DimensionSections sections,
    DimensionSectionMesher sectionMesher,
    DimensionMeshTransferer meshTransferer)
{
    public void Load(Vector3i sloc)
    {
        metrics.SectionMetric.Start();

        sections.TryGet(sloc, out var section);
        sectionMesher.Render(sloc);

        var mesh = section.SectionTerrainMesh();

        if (sectionMesher.Vertices.Length > 0)
            meshTransferer.Transfer(positionColorTextureProgram3D, sectionMesher.Vertices, ref mesh);
        else meshTransferer.Free(ref mesh);

        section.SectionTerrainMesh(mesh);

        sectionMesher.Reset();

        var chunk = chunks[sloc.Xy].GetValueOrDefault();
        var unrendered = chunk.ChunkUnrendered();
        unrendered?.Remove(sloc.Z);

        var rendered = chunk.ChunkRendered();
        if (rendered == null)
        {
            rendered = [];
            chunk.ChunkRendered(rendered);
        }
        rendered.Add(sloc.Z);

        metrics.SectionMetric.End();
    }

    // public void Load2(Entity section)
    // {
    //     metrics.SectionMetric.Start();
    // 
    //     sectionMesher.Render(section.Sloc());
    // 
    //     meshTransferer.Transfer(
    //         positionColorTextureProgram3D,
    //         sectionMesher.Vertices,
    //         ref section.TerrainMesh());
    // 
    //     sectionMesher.Reset();
    // 
    //     section.Chunk().Unrendered().Remove(section.Sloc().Z);
    //     section.Chunk().Rendered().Add(section.Sloc().Z);
    // 
    //     metrics.SectionMetric.End();
    // }
}
