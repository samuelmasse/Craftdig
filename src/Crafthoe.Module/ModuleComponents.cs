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

    // Face
    [ComponentToString] bool IsFace,
    [ComponentToString] string FaceFile,
    [ComponentToString] int FaceIndex
);
