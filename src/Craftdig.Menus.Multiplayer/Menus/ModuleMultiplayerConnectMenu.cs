namespace Craftdig.Menus.Multiplayer;

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
            .Mutate(s.VerticalList)
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Horizontal)
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
                .MaxLengthV(120)
                .StringBuilderV(address)
                .IsInitialFocusV(true);

            Node(form)
                .Mutate(s.Button)
                .MarginV((0, s.ItemHeight, 0, 0))
                .TextV("Join Server")
                .IsInputDisabledF(() => address.Length == 0)
                .OnPressF(() =>
                {
                    var (host, port) = ServerEntry.ParseAddress(address.ToString(), defaultPort);
                    multiplayerConnectAction.Start(host, port);
                    NodeSR(root).Mutate(multiplayerConnectingMenu.Create);
                });

            Node(form)
                .Mutate(s.Button)
                .TextV("Cancel")
                .OnPressF(() => NodeStackPopR(root));
        }
    }
}
