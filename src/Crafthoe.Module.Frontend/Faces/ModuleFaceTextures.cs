namespace Crafthoe.Module.Frontend;

[Module]
public class ModuleFaceTextures(ModuleGlw gl, ModuleImages images)
{
    private readonly Dictionary<string, Texture2D> textures = [];

    public Texture2D this[string file]
    {
        get
        {
            if (!textures.TryGetValue(file, out var value))
            {
                var data = images[file];

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
