namespace Crafthoe.Dimension;

[Components]
file record DimensionComponents(
    // Chunk
    [ComponentToString] bool IsChunk,
    [ComponentToString] Vector2i Cloc,
    [ComponentReturnSpan] Memory<Ent> Blocks,
    [ComponentReturnSpan] Memory<EntPtr> Sections,
    LazySortedList<int, int> Unrendered,
    bool IsUnrenderedListBuilt,
    bool IsReadyToRender,
    LazySortedList<int, int> Rendered,
    int ChunkBagIndex,

    // Section
    [ComponentToString] bool IsSection,
    [ComponentToString] Vector3i Sloc,
    VaoVboCount TerrainMesh,
    EntMut Chunk,

    // Rigid
    Vector3d Position,
    Vector3d PrevPosition,
    Vector3d Velocity,
    [ComponentToString] bool IsRigid,
    int RigidBagIndex
);
