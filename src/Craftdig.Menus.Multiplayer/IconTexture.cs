namespace Craftdig.Menus.Multiplayer;

public class IconTexture(GlBin bin, Texture texture)
{
    public Texture Texture => texture;

    ~IconTexture() => bin.DeleteTexture(texture.Id);
}
