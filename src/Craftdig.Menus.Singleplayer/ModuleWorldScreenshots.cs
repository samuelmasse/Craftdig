namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleWorldScreenshots(RootBin bin, RootGl gl, RootPngs pngs)
{
    private readonly Dictionary<string, ScreenshotTexture?> cache = [];
    private int bucket;

    public void RefillBucket() => bucket = 1;

    public ScreenshotTexture? this[string dir]
    {
        get
        {
            if (cache.TryGetValue(dir, out var texture))
                return texture;

            if (bucket <= 0)
                return null;

            bucket--;
            var file = Path.Join(dir, "Screenshot.png");

            try
            {
                if (File.Exists(file))
                {
                    var image = pngs[file];
                    texture = new(bin, new Texture2D(gl, (Vec2u)image.Size)
                    {
                        PixelsMipmap = image.Pixels.Span,
                        MagFilter = GlTextureMagFilter.Linear,
                        MinFilter = GlTextureMinFilter.LinearMipmapLinear
                    });
                }
            }
            catch { }

            cache[dir] = texture;
            return texture;
        }
    }
}
