namespace Craftdig.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerDisconnectedState(
    RootUi ui,
    RootBackbuffer backbuffer,
    ModuleMainBackgroundMenu mainBackgroundMenu,
    PlayerMultiplayerDisconnectedMenu multiplayerDisconnectedMenu) : State
{
    private readonly EntMut menus = Node(ui);

    public override void Load()
    {
        menus.Mutate().StackRootV(menus);
        Node(menus).Mutate(mainBackgroundMenu.Create);
        NodeSR(menus).Mutate(multiplayerDisconnectedMenu.Create);
    }

    public override void Unload()
    {
        NodesRemove(ui, menus);
    }

    public override void Render() => backbuffer.Clear();
}
