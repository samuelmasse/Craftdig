namespace Crafthoe.Frontend;

[App]
public class AppMenuState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootUi ui,
    AppMainMenu mainMenu) : State
{
    private readonly EntObj menus = Node(ui).SizeRelativeV((1, 1));
    private readonly Stopwatch watch = new();

    public override void Load()
    {
        menus.NodeStack().Push(mainMenu.Get(menus));
        watch.Start();
    }

    public override void Unload()
    {
        ui.Nodes().Remove(menus);
    }

    public override void Update(double time)
    {
        if (watch.ElapsedMilliseconds > 30)
            screen.IsVisible = true;
    }

    public override void Render() => backbuffer.Clear();
}
