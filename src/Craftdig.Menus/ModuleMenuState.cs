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
        NodeS(menus).StackRootV(menus).Mutate(mainMenu.Create);
        watch.Start();
    }

    public override void Unload()
    {
        NodesRemove(ui, menus);
    }

    public override void Update(double time)
    {
        if (keyboard.IsKeyPressed(Keys.Escape))
        {
            if (NodeStackCount(menus) > 1)
                NodeStackPop(menus);
        }

        if (watch.ElapsedMilliseconds > 30)
            screen.IsVisible = true;
    }

    public override void Render() => backbuffer.Clear();
}
