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
    private readonly EntMut menus = Node(ui);
    private readonly Stopwatch watch = new();

    public override void Load()
    {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

        Node(menus).Mutate(mainBackgroundMenu.Create);
        NodeStack(menus).StackRootV(menus).Mutate(mainMenu.Create);
        watch.Start();
    }

    public override void Unload()
    {
        ui.Nodes.Remove(menus);
    }

    public override void Update(double time)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            if (menus.NodeStack.Count > 1)
                menus.NodeStack.Pop();
        }

        if (watch.ElapsedMilliseconds > 30)
            screen.IsVisible = true;
    }

    public override void Render() => backbuffer.Clear();
}
