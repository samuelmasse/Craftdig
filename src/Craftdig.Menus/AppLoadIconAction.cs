namespace Craftdig.Menus;

using BigGustave;

[App]
public class AppLoadIconAction(AppFiles files)
{
    public WindowIcon Run()
    {
        var png = Png.Open(files["Textures/Icon.png"]);

        var data = new byte[png.Width * png.Height * 4];
        for (int y = 0; y < png.Height; y++)
        {
            for (int x = 0; x < png.Width; x++)
            {
                var p = png.GetPixel(x, y);
                int i = (y * png.Width + x) * 4;
                data[i + 0] = p.R;
                data[i + 1] = p.G;
                data[i + 2] = p.B;
                data[i + 3] = p.A;
            }
        }

        return new WindowIcon(new OpenTK.Windowing.Common.Input.Image(png.Width, png.Height, data));
    }
}
