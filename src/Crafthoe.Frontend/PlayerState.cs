namespace Crafthoe.Frontend;

[Player]
public class PlayerState(
    RootCanvas canvas,
    RootMouse mouse,
    RootKeyboard keyboard,
    RootBackbuffer backbuffer,
    RootSprites sprites,
    RootMetrics metrics,
    RootText text,
    RootScale scale,
    RootRoboto roboto,
    PlayerContext context,
    PlayerCamera camera) : State
{
    private bool paused;

    private readonly Func<ReadOnlySpan<char>>[] lines =
    [
        () => text.Format("Frame: {0}. {1:F3} ms ({2} FPS)",
            metrics.Frame.Ticks, metrics.FrameWindow.Average, metrics.FrameWindow.Ticks),
        () => text.Format("Position: {0:F3}", camera.Offset),
        () => text.Format("Rotation: {0:F3}", camera.Rotation)
    ];

    public override void Load()
    {
        context.Load();
    }

    public override void Update(double time)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
            paused = !paused;

        mouse.Track = false;
        if (!paused)
            context.Update(time);

        mouse.CursorState = mouse.Track ? CursorState.Grabbed : CursorState.Normal;
    }

    public override void Render()
    {
        backbuffer.Clear();
        context.Render();
    }

    public override void Draw()
    {
        var fontSize = roboto[scale[22]];

        for (int i = 0; i < lines.Length; i++)
            sprites.Batch.Write(fontSize, lines[i].Invoke(), (scale[17], scale[25] + i * fontSize.Metrics.Height));

        if (paused)
            sprites.Batch.Draw((0, 0), canvas.Size, (0.3f, 0.3f, 0.3f, 0.3f));
    }
}
