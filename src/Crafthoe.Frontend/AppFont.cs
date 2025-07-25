namespace Crafthoe.Frontend;

[App]
public class AppFont(RootFonts fonts, AppFiles files)
{
    private readonly Font font = fonts[files["Fonts/RobotoMono-Regular.ttf"]];

    public Font Value => font;
}
