namespace AlvorEngine.Loop;

[Root]
public class RootGraphics2D(
    RootState state,
    RootGlw gl,
    RootCanvas canvas,
    RootFonts fonts,
    RootSprites sprites,
    RootUi ui,
    RootUiMouse uiMouse,
    RootUiSystem uiSystem)
{
    public void Unload() => fonts.Unload();

    public void Render()
    {
        fonts.Pack();
        sprites.Begin(canvas.Size);
        state.Current.Draw();

        ui.SizeV() = canvas.Size;
        uiSystem.Size(ui.SizeR(), ui);
        uiSystem.Position(ui.SizeR(), ui);
        uiMouse.Update((0, 0), ui);
        uiSystem.Draw(ui.OffsetR(), ui);

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
