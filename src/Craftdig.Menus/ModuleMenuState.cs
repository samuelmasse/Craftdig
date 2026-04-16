namespace Craftdig.Menus;

[Module]
public class ModuleMenuState(
    RootBackbuffer backbuffer,
    RootScreen screen,
    RootUi ui,
    RootKeyboard keyboard,
    AppStyle s,
    ModuleMainBackgroundMenu mainBackgroundMenu,
    ModuleMainMenu mainMenu) : State
{
    private readonly EntMut menus = Node(ui).InnerAlignmentSnapV(s.ItemSpacingXS);
    private readonly Stopwatch watch = new();
    private int update;
    private bool gc;

    public override void Load()
    {
        menus.Mutate().StackRootV(menus);
        Node(menus).Mutate(mainBackgroundMenu.Create);
        PushMenu(menus, mainMenu.Create);
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
                PopMenu(menus);
        }

        if (watch.ElapsedMilliseconds > 30)
            screen.IsVisible = true;

        if (!gc && update >= 3 && watch.ElapsedMilliseconds > 15)
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

            gc = true;
        }

        update++;
    }

    public override void Render() => backbuffer.Clear();
}
