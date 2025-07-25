namespace Crafthoe.Frontend;

[App]
public class AppMenuState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootMouse mouse,
    RootSprites sprites,
    AppFont font) : State
{
    public override void Load()
    {
        screen.Title = "Crafthoe";
        screen.Size = (screen.MonitorSize / 4) * 3;
        screen.IsVsyncOn = true;
        screen.IsVisible = true;
    }

    public override void Render() => backbuffer.Clear();

    public override void Draw()
    {
        sprites.Batch.Draw(mouse.Position, (250, 500), (0, 1, 1, 1));
        sprites.Batch.Write(font.Value.Size(55), "Hello Crafthoe", (50, 50));
    }
}
