namespace Craftdig.Dimension.Frontend;

[Components]
file record DimensionFrontendComponents(
    // Chunk
    [ComponentReturnSpan] Memory<EntPtr> Sections,
    SortedList<int, int> Unrendered,
    bool IsUnrenderedListBuilt,
    bool IsReadyToRender,
    SortedList<int, int> Rendered,

    // Section
    [ComponentToString] bool IsSection,
    [ComponentToString] Vector3i Sloc,
    SectionMesh TerrainMesh,
    EntMut Chunk
);
