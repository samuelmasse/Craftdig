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
        mouse.Update((0, 0), ui);
        focus.Update(ui);
        update.Update(ui);
    }

    public override void Draw()
    {
        ResetRoot();
        Traverse();
        draw.Draw(ui.OffsetR(), ui);
    }

    private void ResetRoot()
    {
        ui.IsOrderedV() = true;
        ui.SizeV() = DrawArea.GetValueOrDefault();
        ui.SizeRelativeV() = (0, 0);
    }

    private void Traverse()
    {
        traverse.Traverse(ui, 0);
        size.Size(ui.SizeR(), ui);
        position.Position(ui.SizeR(), ui);
    }
}
