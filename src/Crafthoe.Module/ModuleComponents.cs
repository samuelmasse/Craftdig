namespace Crafthoe.Module;

[Components]
file record ModuleComponents(
    [ComponentToString] string ModuleName,
    [ComponentToString] string Name,

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
