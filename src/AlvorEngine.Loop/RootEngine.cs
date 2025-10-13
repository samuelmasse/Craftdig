namespace AlvorEngine.Loop;

[Root]
public class RootEngine(
    RootState state,
    RootGlw gl,
    RootGraphics2D graphics2D,
    RootMetrics metrics,
    RootText text,
    RootScripts scripts)
{
    public void Load()
    {
        metrics.Start();
    }

    public void Unload()
    {
        graphics2D.Unload();
        gl.Dispose();
        metrics.Stop();
    }

    public void Update(double time)
    {
        foreach (var script in scripts.Span)
            script.Update(time);

        state.Current.Update(time);
    }

    public void Render()
    {
        foreach (var script in scripts.Span)
            script.Render();

        state.Current.Render();
        graphics2D.Render();
        text.Clear();

        metrics.FrameMetric.End();
        metrics.FrameMetric.Start();
    }
}
