namespace Craftdig;

[Module]
public class ModuleMultiplayerConnectMenu(
    AppStyle s,
    AppClientOptions clientOptions,
    ModuleMultiplayerConnectAction multiplayerConnectAction,
    ModuleMultiplayerConnectingMenu multiplayerConnectingMenu)
{
    public void Create(EntMut root)
    {
        int defaultPort = clientOptions.UseRawTcp ? 36677 : 36676;
        var address = new StringBuilder($"127.0.0.1:{defaultPort}");

        Node(root, out var form)
            .Mutate(s.Form)
            .OffsetV((0, s.ItemSpacingXXL));
        {
            Node(form)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .TextV("Direct Connection");

            Node(form)
                .Mutate(s.Label)
                .MarginV((0, s.ItemHeight, 0, 0))
                .TextV("Server Address");

            Node(form)
                .Mutate(s.Textbox)
                .TextboxMaxLengthV(120)
                .TextboxStringBuilderV(address)
                .IsInitialFocusV(true);

            Node(form)
                .Mutate(s.Button)
                .MarginV((0, s.ItemHeight, 0, 0))
                .TextV("Join Server")
                .IsInputDisabledF(() => address.Length == 0)
                .OnPressF(() =>
                {
                    multiplayerConnectAction.Start(ServerAddress.Parse(address.ToString(), defaultPort));
                    PushMenu(root, multiplayerConnectingMenu.Create);
                });

            Node(form)
                .Mutate(s.Button)
                .TextV("Cancel")
                .OnPressF(() => PopMenu(root));
        }
    }
}
