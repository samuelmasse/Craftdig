namespace Craftdig;

[Player]
public class PlayerMultiplayerDisconnectedState(
    RootUi ui,
    RootBackbuffer backbuffer,
    ModuleMainBackgroundMenu mainBackgroundMenu,
    ModuleMultiplayerDisconnectedMenu multiplayerDisconnectedMenu) : State
{
    private readonly EntMut menus = Node(ui.Default.Root);

    public override void Load()
    {
        menus.Mutate().StackRootV(menus);
        PushMenu(menus, multiplayerDisconnectedMenu.Create, mainBackgroundMenu.Create);
    }

    public override void Unload()
    {
        NodesRemove(ui.Default.Root, menus);
    }

    public override void Render() => backbuffer.Clear();
}
