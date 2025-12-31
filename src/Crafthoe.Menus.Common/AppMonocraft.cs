namespace Craftdig.Menus.Common;

[App]
public class AppMonocraft(RootFonts fonts, AppFiles files)
{
    private readonly Font font = fonts.Open(new() { File = files["Fonts/Monocraft.ttf"] });

    public FontSize this[int size] => font.Size(size);
    public Font Font => font;
}
