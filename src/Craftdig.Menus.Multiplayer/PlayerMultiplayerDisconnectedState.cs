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
        NodeStack(menus).StackRootV(menus).Mutate(multiplayerDisconnectedMenu.Create);
    }

    public override void Unload()
    {
        ui.Nodes.Remove(menus);
    }

    public override void Render() => backbuffer.Clear();
}
