namespace Craftdig.Menus;

[Module]
public class ModuleMenuState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootUi ui,
    RootKeyboard keyboard,
    ModuleMainBackgroundMenu mainBackgroundMenu,
    ModuleMainMenu mainMenu) : State
{
    private readonly EntObj menus = Node(ui);
    private readonly Stopwatch watch = new();

    public override void Load()
    {
        GC.Collect(GC.MaxGeneration);
        Node(menus).Mut(mainBackgroundMenu.Create);
        menus.NodeStack().Push(Node().StackRootV(menus).Mut(mainMenu.Create));
        watch.Start();
    }

    public override void Unload()
    {
        ui.Nodes().Remove(menus);
    }

    public override void Update(double time)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            if (menus.NodeStack().Count > 1)
                menus.NodeStack().Pop();
        }

        if (watch.ElapsedMilliseconds > 30)
            screen.IsVisible = true;
    }

    public override void Render() => backbuffer.Clear();
}
