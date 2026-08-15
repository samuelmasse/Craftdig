namespace Craftdig;

[App]
public class AppLoadIconAction(AppFiles files, RootPngs pngs, RootScreen screen)
{
    public void Run()
    {
        var image = pngs[files["Textures/Icon.png"]];
        screen.SetIcon((Vec2u)image.Size, FlipForWindowIcon(image));
    }

    private static Vec4u8[] FlipForWindowIcon(ImageData image)
    {
        var width = image.Size.X;
        var height = image.Size.Y;
        var source = image.Pixels.Span;
        var pixels = new Vec4u8[source.Length];

        for (var y = 0; y < height; y++)
        {
            var sourceRow = (height - 1 - y) * width;
            var targetRow = y * width;
            source.Slice(sourceRow, width).CopyTo(pixels.AsSpan(targetRow, width));
        }

        return pixels;
    }
}
