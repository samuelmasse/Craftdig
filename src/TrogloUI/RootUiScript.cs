namespace TrogloUI;

[Root]
public class RootUiScript(
    RootCanvas canvas,
    RootUiScale scale,
    RootUiTraverse traverse,
    RootUiSize size,
    RootUiPosition position,
    RootUiDraw draw,
    RootUi ui,
    RootUiMouse mouse,
    RootUiFocus focus,
    RootUiUpdate update) : Script
{
    public override Vector2? DrawArea => canvas.Size / scale.Scale;

    public override void Update(double time)
    {
        ResetRoot();
        Traverse();
        mouse.Update(ui);
        focus.Update(ui);
        update.Update(ui);
        ui.Cleanup();
    }

    public override void Draw()
    {
        ResetRoot();
        Traverse();
        draw.Draw(ui);
    }

    private void ResetRoot()
    {
        ui.IsOrderedFV = true;
        ui.SizeFV = DrawArea.GetValueOrDefault();
        ui.SizeRelativeFV = (Vector2?)(0, 0);
    }

    private void Traverse()
    {
        traverse.Traverse(ui, 0);
        size.Size(ui.SizeR, ui);
        position.Position(ui.SizeR, ui);
        position.Finalize(ui.OffsetR, ui);
    }
}
