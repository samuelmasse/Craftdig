namespace Craftdig;

[Module]
public class ModuleFaceTextures(ModuleGl gl, ModuleImages images)
{
    private readonly Dictionary<string, Texture2D> textures = [];

    public Texture2D this[string file]
    {
        get
        {
            if (!textures.TryGetValue(file, out var value))
            {
                var data = images[file];

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
