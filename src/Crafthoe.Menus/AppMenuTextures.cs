namespace Crafthoe.Menus;

[App]
public class AppMenuTextures(RootPngs pngs, AppFiles files, AppGlw gl)
{
    private readonly Dictionary<string, Texture2D> textures = [];

    public Texture2D this[string file]
    {
        get
        {
            if (!textures.TryGetValue(file, out var value))
            {
                var data = pngs[files[Path.Combine("Textures", file) + ".png"]];

                value = new Texture2D(gl, data.Size)
                {
                    PixelsMipmap = data.Pixels.Span,
                    MagFilter = TextureMagFilter.Nearest,
                    MinFilter = TextureMinFilter.NearestMipmapLinear,
                    WrapS = TextureWrapMode.Repeat,
                    WrapT = TextureWrapMode.Repeat,
                };

                textures.Add(file, value);
            }

            return value;
        }
    }
}
