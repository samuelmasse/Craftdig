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
        Node(menus).Mutate(mainBackgroundMenu.Create);
        NodeS(menus).StackRootV(menus).Mutate(multiplayerDisconnectedMenu.Create);
    }

    public override void Unload()
    {
        NodesRemove(ui, menus);
    }

    public override void Render() => backbuffer.Clear();
}
