namespace Craftdig.Menus.Singleplayer;

public class ScreenshotTexture(GlwBin bin, Texture texture)
{
    public Texture Texture => texture;

    ~ScreenshotTexture() { Console.WriteLine("DELETING " + texture.Id); bin.DeleteTexture(texture.Id); }
}
