namespace Craftdig;

[Module]
public class ModuleMultiplayerConnectingMenu(
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
                            out var socket,
                            out var identitySession))
                    {
                        multiplayerJoinAction.Run(socket, identitySession);
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
