namespace Crafthoe.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerConnectMenu(
    AppStyle s,
    AppClientOptions clientOptions,
    ModuleScope module,
    ModuleMultiplayerCredentials multiplayerCredentials,
    ModuleMultiplayerConnectAction multiplayerConnectAction,
    ModuleMultiplayerConnectingMenu multiplayerConnectingMenu)
{
    public void Create(EntObj root)
    {
        string defaultName = "127.0.0.1";
        string defaultPort = clientOptions.UseRawTcp ? "36677" : "36676";

        var host = new StringBuilder(defaultName);
        var port = new StringBuilder(defaultPort);
        var user = new StringBuilder(clientOptions.NoAuthUser);

        Node(root, out var form)
            .Mut(s.VerticalList)
            .OffsetV((0, s.ItemHeight))
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Horizontal);
        {
            if (clientOptions.NoAuthUser == null)
            {
                Node(form)
                    .Mut(s.Label)
                    .TextV(multiplayerCredentials.Email ?? string.Empty);

                Node(form)
                    .OnPressF(() =>
                    {
                        multiplayerCredentials.Logout();

                        root.StackRootV()?.NodeStack().Pop();
                        root.StackRootV()?.NodeStack().Push(
                            Node().StackRootV(root.StackRootV()).Mut(module.Get<ModuleMultiplayerLoginMenu>().Create));
                    })
                    .TextV("Logout")
                    .Mut(s.Button);
            }
            else
            {
                Node(form)
                    .Mut(s.Label)
                    .TextV("User");

                Node(form)
                    .Mut(s.Textbox)
                    .StringBuilderV(user);
            }

            Node(form)
                .Mut(s.Label)
                .TextV("Host");

            Node(form)
                .Mut(s.Textbox)
                .MaxLengthV(120)
                .StringBuilderV(host)
                .IsInitialFocusV(true);

            Node(form)
                .Mut(s.Label)
                .TextV("Port");

            Node(form)
                .Mut(s.Textbox)
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
                .Mut(s.HorizontalList)
                .AlignmentV(Alignment.Center)
                .OffsetMultiplierV(s.ItemSpacingXS)
                .SizeInnerMaxRelativeV(s.Vertical)
                .InnerSpacingV(s.ItemSpacingL)
                .ColorV(s.BoardColor2);
            {
                Node(buttonsList, out var leftButtonsVertical)
                    .Mut(s.VerticalList)
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

                            root.StackRootV()?.NodeStack().Push(
                                Node().StackRootV(root.StackRootV()).Mut(multiplayerConnectingMenu.Create));
                        })
                        .IsInputDisabledF(() =>
                        {
                            port.CopyTo(0, portChars, port.Length);
                            return !int.TryParse(portChars.AsSpan()[..port.Length], out _);
                        })
                        .TextV("Connect")
                        .Mut(s.Button);
                }

                Node(buttonsList, out var rightButtonsVertical)
                    .Mut(s.VerticalList)
                    .SizeV((s.ItemWidthL, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    Node(rightButtonsVertical)
                        .OnPressF(() => root.StackRootV()?.NodeStack().Pop())
                        .TextV("Cancel")
                        .Mut(s.Button);
                }
            }
        }
    }
}
