namespace Crafthoe.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerConnectingMenu(
    AppStyle s,
    ModuleMultiplayerConnectAction multiplayerConnectAction,
    ModuleMultiplayerJoinAction multiplayerJoinAction)
{
    public void Create(EntObj root)
    {
        Node(root, out var form)
            .Mut(s.VerticalList)
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Center);
        {
            Node(form)
                .Mut(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextF(() =>
                {
                    if (multiplayerConnectAction.Exception != null)
                        return multiplayerConnectAction.Exception.Message;

                    return "Connecting...";
                })
                .OnFrameF(() =>
                {
                    if (multiplayerConnectAction.Connecting)
                        return;

                    if (multiplayerConnectAction.Tcp != null && multiplayerConnectAction.Stream != null)
                    {
                        multiplayerJoinAction.Run(new(
                            multiplayerConnectAction.Tcp,
                            multiplayerConnectAction.Stream));
                    }
                });

            Node(form)
                .OnPressF(() =>
                {
                    multiplayerConnectAction.Cancel();
                    root.StackRootV()?.NodeStack().Pop();
                })
                .TextV("Cancel")
                .Mut(s.Button);
        }
    }
}
