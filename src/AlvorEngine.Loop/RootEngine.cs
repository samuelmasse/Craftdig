namespace AlvorEngine.Loop;

[Root]
public class RootEngine(
    RootState state,
    RootGlw gl,
    RootGraphics2D graphics2D,
    RootMetrics metrics,
    RootText text,
    RootCanvas canvas,
    RootUiSystem uiSystem,
    RootUi ui,
    RootUiMouse uiMouse)
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
        ui.IsOrderedV() = true;
        ui.SizeV() = canvas.Size;
        uiSystem.Traverse(ui, 0);
        uiSystem.Size(ui.SizeR(), ui);
        uiSystem.Position(ui.SizeR(), ui);
        uiMouse.Update((0, 0), ui);

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
