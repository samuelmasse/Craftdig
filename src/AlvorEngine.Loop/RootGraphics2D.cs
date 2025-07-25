namespace AlvorEngine.Loop;

[Root]
public class RootGraphics2D(RootState state, RootGlw gl, RootCanvas canvas, RootFonts fonts, RootSprites sprites)
{
    public void Unload() => fonts.Unload();

    public void Render()
    {
        fonts.Pack();
        sprites.Begin(canvas.Size);
        state.Current.Draw();

        gl.Viewport(canvas.Size);

        gl.Enable(EnableCap.Blend);
        gl.Enable(EnableCap.CullFace);

        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        gl.CullFace(TriangleFace.Back);

        sprites.End();

        gl.ResetBlendFunc();
        gl.ResetCullFace();

        gl.Disable(EnableCap.Blend);
        gl.Disable(EnableCap.CullFace);

        gl.ResetViewport();
    }
}
