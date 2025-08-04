namespace Crafthoe.Module;

[Components]
file record ModuleComponents(
    [ComponentToString] long ModuleId,
    [ComponentToString] string ModuleName,

    // Block
    [ComponentToString] bool IsBlock,
    BlockFaces Faces,
    bool IsSolid,

    // Face
    [ComponentToString] bool IsFace,
    [ComponentToString] string FaceFile,
    [ComponentToString] int FaceIndex
);
