namespace Craftdig.Menus.Singleplayer;

public class ScreenshotTexture(GlwBin bin, Texture texture)
{
    public Texture Texture => texture;

    ~ScreenshotTexture() => bin.DeleteTexture(texture.Id);
}
