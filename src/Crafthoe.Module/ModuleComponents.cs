namespace Crafthoe.Module;

[Components]
file record ModuleComponents(
    [ComponentToString] string ModuleName,
    [ComponentToString] string Name,
    int RuntimeIndex,

    [ComponentToString] bool IsDifficulty,
    float Order,

    // Game mode
    [ComponentToString] bool IsGameMode,
    Ent LockedDifficulty,

    // Dimension
    [ComponentToString] bool IsDimension,
    Ent Air,
    Type TerrainGeneratorType,
    Type BiomeGeneraetorType,

    // Block
    [ComponentToString] bool IsBlock,
    bool IsBuildable,
    BlockFaces Faces,
    bool IsSolid,
    int MaxStack,

    // Face
    [ComponentToString] bool IsFace,
    [ComponentToString] string FaceFile,
    [ComponentToString] int FaceIndex,
    Texture2D? FaceTexture
);
