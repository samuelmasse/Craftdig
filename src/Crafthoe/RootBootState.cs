namespace Crafthoe;

[Root]
public class RootBootState(RootScreen screen, RootCanvas canvas, RootMouse mouse, RootGlw glw, RootSprites sprites) : State
{
    public override void Load()
    {
        screen.IsVsyncOn = true;
        screen.Title = "Crafthoe";
        screen.Size = (screen.MonitorSize / 4) * 3;
        screen.IsVisible = true;
    }

    public override void Render()
    {
        glw.Viewport(canvas.Size);
        glw.ClearColor((1, 0, 0, 0));

        glw.Clear(ClearBufferMask.ColorBufferBit);

        glw.ResetClearColor();
        glw.ResetViewport();
    }

    public override void Draw()
    {
        sprites.Batch.Draw(mouse.Position, (250, 500), (0, 1, 1, 1));
    }
}
