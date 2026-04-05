namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerConnectMenu(
    AppStyle s,
    AppClientOptions clientOptions,
    ModuleScope module,
    ModuleMultiplayerCredentials multiplayerCredentials,
    ModuleMultiplayerConnectAction multiplayerConnectAction,
    ModuleMultiplayerConnectingMenu multiplayerConnectingMenu)
{
    public void Create(EntMut root)
    {
        string defaultName = "127.0.0.1";
        string defaultPort = clientOptions.UseRawTcp ? "36677" : "36676";

        var host = new StringBuilder(defaultName);
        var port = new StringBuilder(defaultPort);
        var user = new StringBuilder(clientOptions.NoAuthUser);

        Node(root, out var form)
            .Mutate(s.VerticalList)
            .OffsetV((0, s.ItemHeight))
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Horizontal);
        {
            if (clientOptions.NoAuthUser == null)
            {
                Node(form)
                    .Mutate(s.Label)
                    .TextV(multiplayerCredentials.Email ?? string.Empty);

                Node(form)
                    .OnPressF(() =>
                    {
                        multiplayerCredentials.Logout();

                        NodeStackPopR(root);
                        NodeSR(root).Mutate(module.Get<ModuleMultiplayerLoginMenu>().Create);
                    })
                    .TextV("Logout")
                    .Mutate(s.Button);
            }
            else
            {
                Node(form)
                    .Mutate(s.Label)
                    .TextV("User");

                Node(form)
                    .Mutate(s.Textbox)
                    .StringBuilderV(user);
            }

            Node(form)
                .Mutate(s.Label)
                .TextV("Host");

            Node(form)
                .Mutate(s.Textbox)
                .MaxLengthV(120)
                .StringBuilderV(host)
                .IsInitialFocusV(true);

            Node(form)
                .Mutate(s.Label)
                .TextV("Port");

            Node(form)
                .Mutate(s.Textbox)
                .MaxLengthV(6)
                .StringBuilderV(port);
        }

        Node(root, out var bottomBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight - s.ItemHeight))
            .AlignmentV(Alignment.Horizontal | Alignment.Bottom)
            .ColorV(s.BoardColor);
        {
            Node(bottomBar, out var buttonsList)
                .Mutate(s.HorizontalList)
                .AlignmentV(Alignment.Center)
                .OffsetMultiplierV(s.ItemSpacingXS)
                .SizeInnerMaxRelativeV(s.Vertical)
                .InnerSpacingV(s.ItemSpacingL)
                .ColorV(s.BoardColor2);
            {
                Node(buttonsList, out var leftButtonsVertical)
                    .Mutate(s.VerticalList)
                    .SizeV((s.ItemWidthL, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    var portChars = new char[byte.MaxValue];

                    Node(leftButtonsVertical)
                        .OnPressF(() =>
                        {
                            if (clientOptions.NoAuthUser != null)
                            {
                                clientOptions.NoAuthUser = user.ToString();
                                clientOptions.DefaultNoAuthUser = clientOptions.NoAuthUser;
                            }

                            string connHost = host.ToString();
                            int connPort = int.Parse(port.ToString());

                            multiplayerConnectAction.Start(connHost, connPort);

                            NodeSR(root).Mutate(multiplayerConnectingMenu.Create);
                        })
                        .IsInputDisabledF(() =>
                        {
                            port.CopyTo(0, portChars, port.Length);
                            return !int.TryParse(portChars.AsSpan()[..port.Length], out _);
                        })
                        .TextV("Connect")
                        .Mutate(s.Button);
                }

                Node(buttonsList, out var rightButtonsVertical)
                    .Mutate(s.VerticalList)
                    .SizeV((s.ItemWidthL, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    Node(rightButtonsVertical)
                        .OnPressF(() => NodeStackPopR(root))
                        .TextV("Cancel")
                        .Mutate(s.Button);
                }
            }
        }
    }
}
