namespace Craftdig;

[App]
public class AppLoadIconAction(AppFiles files, RootPngs pngs, RootScreen screen)
{
    public void Run()
    {
        using var stream = File.OpenRead(files["Textures/Icon.png"]);
        var image = pngs.Read(stream).FlippedVertically();
        screen.SetIcon((Vec2u)image.Size, image.Pixels.Span);
    }
}
