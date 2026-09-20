namespace Craftdig;

[App]
public class AppLoadIconAction(AppFiles files, RootPngs pngs, RootScreen screen)
{
    public void Run()
    {
        var image = pngs[files["Textures/Icon.png"]].FlippedVertically();
        screen.SetIcon((Vec2u)image.Size, image.Pixels.Span);
    }
}
