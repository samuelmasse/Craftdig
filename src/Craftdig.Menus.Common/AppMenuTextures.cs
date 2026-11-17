namespace Craftdig;

[App]
public class AppMenuTextures(RootPngs pngs, AppFiles files, AppGl gl)
{
    private readonly Dictionary<string, Texture2D> textures = [];

    public Texture2D this[string file]
    {
        get
        {
            if (!textures.TryGetValue(file, out var value))
            {
                using var stream = File.OpenRead(files[Path.Combine("Textures", file) + ".png"]);
                var data = pngs.Read(stream);

                value = new Texture2D(gl, (Vec2u)data.Size)
                {
                    PixelsMipmap = data.Pixels.Span,
                    MagFilter = GlTextureMagFilter.Nearest,
                    MinFilter = GlTextureMinFilter.NearestMipmapLinear,
                    WrapS = GlTextureWrapMode.Repeat,
                    WrapT = GlTextureWrapMode.Repeat,
                };

                textures.Add(file, value);
            }

            return value;
        }
    }
}
