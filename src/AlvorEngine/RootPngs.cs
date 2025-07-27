namespace AlvorEngine;

[Root]
public class RootPngs
{
    public ImageData this[string file]
    {
        get
        {
            var image = Png.Open(file);
            return new((image.Width, image.Height), GetPixels(image));
        }
    }

    private (byte, byte, byte, byte)[] GetPixels(Png image)
    {
        var pixels = new (byte, byte, byte, byte)[image.Width * image.Height];

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pix = image.GetPixel(x, y);
                pixels[(image.Height - 1 - y) * image.Width + x] = (pix.R, pix.G, pix.B, pix.A);
            }
        }

        return pixels;
    }
}
