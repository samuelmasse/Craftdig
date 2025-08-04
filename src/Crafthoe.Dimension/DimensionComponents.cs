namespace Crafthoe.Dimension;

[Components]
file record DimensionComponents(
    Vector3d Position,

    // Chunk
    [ComponentToString] bool IsChunk,
    [ComponentToString] Vector2i Cloc,
    [ComponentReturnSpan] Memory<Ent> Blocks,
    [ComponentReturnSpan] Memory<EntPtr> Sections,
    LazySortedList<int, int> Unrendered,
    bool IsUnrendered,
    LazySortedList<int, int> Rendered,

    // Section
    [ComponentToString] bool IsSection,
    [ComponentToString] Vector3i Sloc,
    VaoVboCount TerrainMesh,
    EntMut Chunk);
