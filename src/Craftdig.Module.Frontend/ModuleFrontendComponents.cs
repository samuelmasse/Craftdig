namespace Craftdig.Module.Frontend;

[Components]
file record ModuleFrontendComponents(
    // Block
    BlockFaces Faces,

    // Face
    [ComponentToString] bool IsFace,
    [ComponentToString] string FaceFile,
    [ComponentToString] int FaceIndex,
    Texture2D? FaceTexture
);
