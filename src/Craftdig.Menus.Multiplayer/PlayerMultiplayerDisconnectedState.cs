namespace Craftdig;

[Player]
public class PlayerMultiplayerDisconnectedState(
    AppMenuVisibility visibility,
    RootBackbuffer backbuffer,
    ModuleMainBackgroundMenu mainBackgroundMenu,
    ModuleMultiplayerDisconnectedMenu multiplayerDisconnectedMenu) : State
{
    private readonly EntMut menus = Node(visibility.Root);

    public override void Load()
    {
        menus.Mutate().StackRootV(menus);
        PushMenu(menus, multiplayerDisconnectedMenu.Create, mainBackgroundMenu.Create);
    }

    public override void Unload()
    {
        NodesRemove(visibility.Root, menus);
    }

    public override void Render() => backbuffer.Clear();
}
