namespace Craftdig;

[Components]
public interface IModuleFrontendComponents
{
    // Block
    BlockFaces Faces { get; set; }

    // Face
    [ComponentToString] bool IsFace { get; set; }
    [ComponentToString] string FaceFile { get; set; }
    [ComponentToString] int FaceIndex { get; set; }
    Texture2D? FaceTexture { get; set; }
}
