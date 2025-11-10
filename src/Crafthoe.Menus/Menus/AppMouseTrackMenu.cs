namespace Crafthoe.Menus;

[App]
public class AppMouseTrackMenu(RootMouse mouse)
{
    public void Create(EntObj root)
    {
        Node(root, out var text)
            .OnUpdateF(() =>
            {
                mouse.CursorState = mouse.Track ? CursorState.Grabbed : CursorState.Normal;
                mouse.Track = false;
            });
    }
}
