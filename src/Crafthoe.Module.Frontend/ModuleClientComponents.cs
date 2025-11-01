namespace Crafthoe.Module.Frontend;

[Components]
file record ModuleClientComponents(
    // Block
    BlockFaces Faces,

    // Face
    [ComponentToString] bool IsFace,
    [ComponentToString] string FaceFile,
    [ComponentToString] int FaceIndex,
    Texture2D? FaceTexture
);
