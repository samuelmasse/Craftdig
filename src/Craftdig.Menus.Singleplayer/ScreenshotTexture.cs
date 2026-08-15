namespace Craftdig;

public class ScreenshotTexture(GlBin bin, Texture texture)
{
    public Texture Texture => texture;

    ~ScreenshotTexture() => bin.DeleteTexture(texture.Id);
}
