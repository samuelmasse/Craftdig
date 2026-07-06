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
    private int frame;
    private bool gc;

    public override void Load()
    {
        menus.Mutate().StackRootV(menus);
        PushMenu(menus, mainMenu.Create, mainBackgroundMenu.Create);
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
            if (NodeStackCount(menus) > 1 && NodeStackTryPeek(menus, out var top) && !top.IsModalFV.Resolve())
                PopMenu(menus);
        }
    }

    public override void Frame(double time)
    {
        if (watch.ElapsedMilliseconds > 30)
            screen.IsVisible = true;

        if (!gc && frame >= 3 && watch.ElapsedMilliseconds > 15)
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);

            gc = true;
        }

        frame++;
    }

    public override void Render() => backbuffer.Clear();
}
