namespace AlvorEngine.Loop;

[Root]
public class RootEngine(RootState state, RootGlw gl, RootGraphics2D graphics2D)
{
    public void Load()
    {

    }

    public void Unload()
    {
        graphics2D.Unload();
        gl.Dispose();
    }

    public void Update(double time)
    {
        state.Current.Update(time);
    }

    public void Render()
    {
        state.Current.Render();
        graphics2D.Render();
    }
}
