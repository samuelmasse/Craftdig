namespace Crafthoe.Frontend;

[Player]
public class PlayerState(
    RootGlw gl,
    RootCanvas canvas,
    RootMouse mouse,
    RootKeyboard keyboard,
    RootSprites sprites,
    RootMetrics metrics,
    RootText text,
    RootScale scale,
    RootRoboto roboto,
    DimensionContext dimension,
    DimensionMetrics dimensionsMetrics,
    PlayerContext playerContext,
    PlayerEntity playerEntity,
    PlayerCamera camera,
    PlayerSelected selected) : State
{
    private bool paused;

    private readonly Func<ReadOnlySpan<char>>[] lines =
    [
        () => text.Format("Frame: {0}. {1:F3} ms ({2} FPS)",
            metrics.Frame.Ticks, metrics.FrameWindow.Average, metrics.FrameWindow.Ticks),
        () => text.Format("Position: {0:F3}", playerEntity.Entity.Position()),
        () => text.Format("Rotation: {0:F3}", camera.Rotation),
        () => text.Format("Spike: {0}", metrics.Frame.Max),
        () => text.Format("Render: {0}", dimensionsMetrics.RenderMetric.Value.Max),
        () => text.Format("Chunk: {0}", dimensionsMetrics.ChunkMetric.Value.Max),
        () => text.Format("Section: {0}", dimensionsMetrics.SectionMetric.Value.Max),
        () => text.Format("Buffers: {0}", gl.BufferTotalUsage),
        () => text.Format("Selected Loc: {0}", selected.Loc.GetValueOrDefault()),
        () => text.Format("Selected Normal: {0}", selected.Normal.GetValueOrDefault())
    ];

    public override void Load()
    {
        playerContext.Load();
    }

    public override void Update(double time)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
            paused = !paused;

        mouse.Track = false;
        if (!paused)
        {
            dimension.Update(time);
            playerContext.Update(time);
        }

        mouse.CursorState = mouse.Track ? CursorState.Grabbed : CursorState.Normal;
    }

    public override void Render()
    {
        dimension.Tick();
        playerContext.Render();
    }

    public override void Draw()
    {
        var fontSize = roboto[scale[22]];

        for (int i = 0; i < lines.Length; i++)
            sprites.Batch.Write(fontSize, lines[i].Invoke(), (scale[17], scale[25] + i * fontSize.Metrics.Height));

        if (paused)
            sprites.Batch.Draw((0, 0), canvas.Size, (0.3f, 0.3f, 0.3f, 0.3f));

        float cht = scale[4];
        float chl = cht * 9;
        var c = canvas.Size / 2;

        sprites.Batch.Draw(c - (cht / 2, chl / 2), (cht, chl));
        sprites.Batch.Draw(c - (chl / 2, cht / 2), (chl, cht));
    }
}
