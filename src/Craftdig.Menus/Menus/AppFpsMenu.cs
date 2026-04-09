namespace Craftdig.Menus;

[App]
public class AppFpsMenu(RootKeyboard keyboard, RootText text, RootMetrics metrics, AppStyle s)
{
    public void Create(EntMut root)
    {
        Node(root, out var label)
            .Mutate(s.Label)
            .AlignmentV(Alignment.Top | Alignment.Left)
            .IsDisabledV(true)
            .TextF(() => text.Format("{0} FPS", metrics.FrameWindow.Ticks))
            .ColorV((0.5f, 0.5f, 0.5f, 0.5f))
            .SizeV((s.ItemSpacing, s.ItemSpacing));

        Node(root).OnUpdateF(() =>
        {
            if (keyboard.IsKeyPressed(Keys.F9))
                label.IsDisabledFV = !label.IsDisabledFV.Resolve();
        });
    }
}
