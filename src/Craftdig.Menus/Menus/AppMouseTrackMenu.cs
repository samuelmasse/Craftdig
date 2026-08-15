namespace Craftdig;

[App]
public class AppMouseTrackMenu(RootMouse mouse, RootInput input)
{
    public void Create(EntMut root)
    {
        Node(root, out var text)
            .OnUpdateF(() =>
            {
                input.CursorMode = mouse.Track ? CursorMode.Disabled : CursorMode.Normal;
            });
    }
}
