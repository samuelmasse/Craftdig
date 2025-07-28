namespace Crafthoe.Dimension;

[Components]
file record DimensionComponents(
    [ComponentToString] bool IsChunk,
    [ComponentToString] Vector2i ChunkLocation,
    [ComponentReturnSpan] Memory<ReadOnlyEntity> ChunkBlocks,
    [ComponentReturnSpan] Memory<Entity> ChunkSections,
    [ComponentToString] bool IsSection,
    [ComponentToString] Vector3i SectionLocation,
    VaoVboCount SectionTerrainMesh,
    bool SectionTerrainGenerated);
