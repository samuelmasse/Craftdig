namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerConnectingMenu(
    Log log,
    AppStyle s,
    ModuleMultiplayerConnectAction multiplayerConnectAction,
    ModuleMultiplayerJoinAction multiplayerJoinAction)
{
    public void Create(EntMut root)
    {
        Node(root, out var form)
            .Mutate(s.Form)
            .AlignmentV(Alignment.Center);
        {
            Node(form)
                .Mutate(s.Label)
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

                    if (multiplayerConnectAction.TryTakeConnection(
                            out var tcp,
                            out var stream,
                            out var identitySession))
                    {
                        multiplayerJoinAction.Run(new(
                            log,
                            tcp,
                            stream), identitySession);
                    }
                });

            Node(form)
                .OnPressF(() =>
                {
                    multiplayerConnectAction.Cancel();
                    PopMenu(root);
                })
                .TextV("Cancel")
                .Mutate(s.Button);
        }
    }
}
