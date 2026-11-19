namespace Craftdig;

[App]
public class AppMenuVisibility(RootUi ui, RootKeyboard keyboard, RootMouse mouse, RootScreen screen)
{
    public EntMut Root { get; } = Node(ui.Default.Root)
        .IsOrderedV(true);

    public bool Hidden => Root.IsDisabledFV.Resolve();

    public void Update()
    {
        if (!screen.IsFocused)
            return;

        bool hidden = Hidden;
        if (!keyboard.IsKeyPressed(Keys.F1) && !(hidden && mouse.IsMainPressed()))
            return;

        if (!hidden)
        {
            ui.Mouse.Release();
            ui.Focus.Focus(default, false);
        }
        Root.Mutate().IsDisabledV(!hidden);
    }
}
