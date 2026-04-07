namespace Craftdig.Menus.Multiplayer;

using BigGustave;

[Module]
public class ModuleMultiplayerServerIcons(RootBin bin, RootGlw gl)
{
    private readonly Dictionary<byte[], IconTexture?> textures = [];

    public Texture? this[byte[] bytes]
    {
        get
        {
            if (!textures.TryGetValue(bytes, out var texture))
            {
                texture = Load(bytes);
                textures[bytes] = texture;
            }

            return texture?.Texture;
        }
    }

    private IconTexture? Load(byte[] bytes)
    {
        try
        {
            var png = Png.Open(bytes);
            var pixels = GetPixels(png);

            var texture = new Texture2D(gl, (png.Width, png.Height))
            {
                PixelsMipmap = pixels,
                MagFilter = TextureMagFilter.Nearest,
                MinFilter = TextureMinFilter.NearestMipmapLinear
            };

            return new(bin, texture);
        }
        catch
        {
            return null;
        }
    }

    private (byte, byte, byte, byte)[] GetPixels(Png image)
    {
        (byte, byte, byte, byte)[] array = new (byte, byte, byte, byte)[image.Width * image.Height];
        for (int i = 0; i < image.Height; i++)
        {
            for (int j = 0; j < image.Width; j++)
            {
                Pixel pixel = image.GetPixel(j, i);
                array[(image.Height - 1 - i) * image.Width + j] = (pixel.R, pixel.G, pixel.B, pixel.A);
            }
        }

        return array;
    }
}
