namespace Crafthoe.Frontend;

[Player]
public class PlayerState(
    RootGlw gl,
    RootCanvas canvas,
    RootMouse mouse,
    RootKeyboard keyboard,
    RootBackbuffer backbuffer,
    RootSprites sprites,
    RootMetrics metrics,
    RootText text,
    RootScale scale,
    RootRoboto roboto,
    DimensionContext dimension,
    DimensionMetrics dimensionsMetrics,
    PlayerContext player,
    PlayerCamera camera) : State
{
    private bool paused;

    private readonly Func<ReadOnlySpan<char>>[] lines =
    [
        () => text.Format("Frame: {0}. {1:F3} ms ({2} FPS)",
            metrics.Frame.Ticks, metrics.FrameWindow.Average, metrics.FrameWindow.Ticks),
        () => text.Format("Position: {0:F3}", camera.Offset),
        () => text.Format("Rotation: {0:F3}", camera.Rotation),
        () => text.Format("Spike: {0}", metrics.Frame.Max),
        () => text.Format("Render: {0}", dimensionsMetrics.RenderMetric.Value.Max),
        () => text.Format("Chunk: {0}", dimensionsMetrics.ChunkMetric.Value.Max),
        () => text.Format("Section: {0}", dimensionsMetrics.SectionMetric.Value.Max),
        () => text.Format("Buffers: {0}", gl.BufferTotalUsage)
    ];

    public override void Load()
    {
        player.Load();
    }

    public override void Update(double time)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
            paused = !paused;

        mouse.Track = false;
        if (!paused)
        {
            dimension.Update(time);
            player.Update(time);
        }

        mouse.CursorState = mouse.Track ? CursorState.Grabbed : CursorState.Normal;
    }

    public override void Render()
    {
        dimension.Tick();
        backbuffer.Clear();
        player.Render();
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
