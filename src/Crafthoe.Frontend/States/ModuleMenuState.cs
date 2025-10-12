namespace Crafthoe.Frontend;

[Module]
public class ModuleMenuState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootUi ui,
    ModuleMainMenu mainMenu) : State
{
    private readonly EntObj menus = Node(ui);
    private readonly Stopwatch watch = new();

    public override void Load()
    {
        menus.NodeStack().Push(Node().StackRootV(menus).Mut(mainMenu.Create));
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
