namespace AlvorEngine;

[Root]
public class RootBackbuffer(RootGlw gl, RootCanvas canvas)
{
    public void Clear(Vector4 color = default)
    {
        gl.Viewport(canvas.Size);
        gl.ClearColor(color);

        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        gl.ResetClearColor();
        gl.ResetViewport();
    }
}
