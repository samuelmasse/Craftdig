namespace Craftdig.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerDisconnectedState(
    RootUi ui,
    RootBackbuffer backbuffer,
    ModuleMainBackgroundMenu mainBackgroundMenu,
    PlayerMultiplayerDisconnectedMenu multiplayerDisconnectedMenu) : State
{
    private readonly EntObj menus = Node(ui);

    public override void Load()
    {
        Node(menus).Mut(mainBackgroundMenu.Create);
        menus.NodeStack().Push(Node().StackRootV(menus).Mut(multiplayerDisconnectedMenu.Create));
    }

    public override void Unload()
    {
        ui.Nodes().Remove(menus);
    }

    public override void Render() => backbuffer.Clear();
}
