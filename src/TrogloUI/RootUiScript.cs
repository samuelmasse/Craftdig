namespace TrogloUI;

[Root]
public class RootUiScript(
    RootCanvas canvas,
    RootUiScale scale,
    RootUiTraverse uiTraverse,
    RootUiSystem uiSystem,
    RootUi ui,
    RootUiMouse uiMouse,
    RootUiFocus uiFocus) : Script
{
    public override Vector2? DrawArea => canvas.Size / scale.Scale;

    public override void Update(double time)
    {
        ui.IsOrderedV() = true;
        ui.SizeV() = canvas.Size / scale.Scale;
        ui.SizeRelativeV() = (0, 0);
        uiTraverse.Traverse(ui, 0);
        uiSystem.Size(ui.SizeR(), ui);
        uiSystem.Position(ui.SizeR(), ui);
        uiMouse.Update((0, 0), ui);
        uiFocus.Update(ui);
        uiSystem.Update(ui);
    }

    public override void Draw()
    {
        ui.SizeV() = canvas.Size / scale.Scale;
        uiTraverse.Traverse(ui, 0);
        uiSystem.Size(ui.SizeR(), ui);
        uiSystem.Position(ui.SizeR(), ui);
        uiSystem.Draw(ui.OffsetR(), ui);
    }
}
