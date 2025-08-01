namespace Crafthoe.Dimension;

[Components]
file record DimensionComponents(
    Vector3 Position,
    [ComponentToString] bool IsChunk,
    [ComponentToString] Vector2i ChunkLocation,
    [ComponentReturnSpan] Memory<ReadOnlyEntity> ChunkBlocks,
    [ComponentReturnSpan] Memory<Entity> ChunkSections,
    SortedSet<int>? ChunkUnrendered,
    SortedSet<int>? ChunkRendered,
    [ComponentToString] bool IsSection,
    [ComponentToString] Vector3i SectionLocation,
    VaoVboCount SectionTerrainMesh);
