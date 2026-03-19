namespace Craftdig.Dimension.Frontend;

[Components]
public interface IDimensionFrontendComponents
{
    // Chunk
    Memory<EntPtr> Sections { get; set; }
    SortedList<int, int> Unrendered { get; set; }
    bool IsUnrenderedListBuilt { get; set; }
    bool IsReadyToRender { get; set; }
    SortedList<int, int> Rendered { get; set; }

    // Section
    [ComponentToString] bool IsSection { get; set; }
    [ComponentToString] Vector3i Sloc { get; set; }
    SectionMesh TerrainMesh { get; set; }
    EntMut Chunk { get; set; }
}
