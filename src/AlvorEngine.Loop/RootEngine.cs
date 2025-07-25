namespace AlvorEngine.Loop;

[Root]
public class RootEngine(
    RootState state,
    RootGlw gl,
    RootGraphics2D graphics2D,
    RootMetrics metrics,
    RootText text)
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
        state.Current.Update(time);
    }

    public void Render()
    {
        state.Current.Render();
        graphics2D.Render();
        metrics.FrameMetric.End();
        metrics.FrameMetric.Start();
        text.Clear();
    }
}
