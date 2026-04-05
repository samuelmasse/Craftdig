namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerServerBrowserMenu(
    AppStyle s,
    AppClientOptions clientOptions,
    ModuleScope module,
    ModuleMultiplayerServerList serverList,
    ModuleMultiplayerCredentials multiplayerCredentials,
    ModuleMultiplayerConnectAction multiplayerConnectAction,
    ModuleMultiplayerConnectMenu connectMenu,
    ModuleMultiplayerConnectingMenu connectingMenu)
{
    public void Create(EntMut root)
    {
        ServerEntry? selected = null;
        StringBuilder? user = clientOptions.NoAuthUser != null ? new(clientOptions.NoAuthUser) : null;

        Node(root, out var topBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight))
            .ColorV(s.BoardColor);
        {
            Node(topBar, out var topBarContent)
                .Mutate(s.VerticalList)
                .InnerSpacingV(s.ItemSpacing)
                .SizeInnerMaxRelativeV((1, 0))
                .AlignmentV(Alignment.Center);
            {
                Node(topBarContent)
                    .Mutate(s.Label)
                    .AlignmentV(Alignment.Horizontal)
                    .TextV("Play Multiplayer");

                if (user != null)
                {
                    Node(topBarContent)
                        .Mutate(s.Textbox)
                        .SizeV((s.ItemWidthL, s.ItemHeight))
                        .StringBuilderV(user)
                        .OnTextUpdatedF(() =>
                        {
                            clientOptions.NoAuthUser = user.ToString();
                            clientOptions.DefaultNoAuthUser = clientOptions.NoAuthUser;
                        });
                }
                else
                {
                    Node(topBarContent)
                        .Mutate(s.Label)
                        .AlignmentV(Alignment.Horizontal)
                        .TextColorV(s.TextColorFaint)
                        .TextV(multiplayerCredentials.Email ?? string.Empty);
                }
            }

            if (user == null)
            {
                Node(topBar)
                    .Mutate(s.Button)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidth, s.ItemHeight))
                    .AlignmentV(Alignment.Top | Alignment.Right)
                    .OffsetV((-s.ItemSpacingS, s.ItemSpacingS))
                    .TextV("Logout")
                    .OnPressF(() =>
                    {
                        multiplayerCredentials.Logout();
                        NodeStackPopR(root);
                        NodeSR(root).Mutate(module.Get<ModuleMultiplayerLoginMenu>().Create);
                    });
            }
        }

        Node(root, out var middle)
            .SizeRelativeV((1, 1))
            .SizeV((0, -s.BarHeight * 2))
            .OffsetV((0, s.BarHeight));
        {
            s.Selector(middle, out var select);

            for (int i = 0; i < serverList.Servers.Length; i++)
            {
                var server = serverList.Servers[i];
                var itemHeight = s.ItemHeight * 1.5f;

                Node(select, out var item)
                    .Mutate(s.SelectorItem)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL * 1.7f, itemHeight + s.ItemSpacingS * 2))
                    .OnFocusF(() => selected = server)
                    .OnUnselectF(() => selected = null)
                    .OnDoubleClickF(Connect)
                    .FocusGroupV(select);
                {
                    Node(item, out var itemContainer)
                        .SizeRelativeV((1, 1))
                        .PaddingV((s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS));
                    {
                        Node(itemContainer)
                            .SizeRelativeV((0, 0))
                            .SizeV((itemHeight, itemHeight))
                            .ColorV((0.2f, 0, 0.6f, 1));

                        Node(itemContainer, out var itemList)
                            .Mutate(s.VerticalList)
                            .SizeRelativeV((1, 0))
                            .OffsetV((itemHeight + s.ItemSpacingS, 0))
                            .SizeV((-itemHeight - s.ItemSpacingS, 0))
                            .AlignmentV(Alignment.Left | Alignment.Vertical);
                        {
                            Node(itemList)
                                .Mutate(s.Label)
                                .SizeV((0, s.ItemSpacingS))
                                .TextV(server.Name);

                            Node(itemList)
                                .Mutate(s.Label)
                                .TextColorV(s.TextColorFaint)
                                .TextV($"{server.Host}:{server.Port}");
                        }
                    }
                }
            }
        }

        Node(root, out var bottomBar)
            .SizeRelativeV(s.Horizontal)
            .SizeV((0, s.BarHeight))
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
                Node(buttonsList, out var leftButtons)
                    .Mutate(s.VerticalList)
                    .SizeV((s.ItemWidthL * 2.5f, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    Node(leftButtons, out var topRow)
                        .SizeRelativeV(s.Horizontal)
                        .SizeInnerMaxRelativeV(s.Vertical)
                        .InnerSpacingV(s.ItemSpacing)
                        .InnerLayoutV(InnerLayout.HorizontalList)
                        .InnerSizingV(InnerSizing.HorizontalWeight);
                    {
                        Node(topRow)
                            .Mutate(s.Button)
                            .TextV("Join Server")
                            .IsInputDisabledF(() => selected == null)
                            .OnPressF(Connect);

                        Node(topRow)
                            .Mutate(s.Button)
                            .TextV("Direct Connection")
                            .OnPressF(() => NodeSR(root).Mutate(connectMenu.Create));

                        Node(topRow)
                            .Mutate(s.Button)
                            .TextV("Add Server")
                            .OnPressF(() => NodeSR(root).Mutate(r =>
                                module.Get<ModuleMultiplayerAddServerMenu>().Create(r, null)));
                    }

                    Node(leftButtons, out var bottomRow)
                        .SizeRelativeV(s.Horizontal)
                        .SizeInnerMaxRelativeV(s.Vertical)
                        .InnerSpacingV(s.ItemSpacing)
                        .InnerLayoutV(InnerLayout.HorizontalList)
                        .InnerSizingV(InnerSizing.HorizontalWeight);
                    {
                        Node(bottomRow)
                            .Mutate(s.Button)
                            .TextV("Edit")
                            .IsInputDisabledF(() => selected == null)
                            .OnPressF(() => NodeSR(root).Mutate(r =>
                                module.Get<ModuleMultiplayerAddServerMenu>().Create(r, selected)));

                        Node(bottomRow)
                            .Mutate(s.Button)
                            .TextV("Delete")
                            .IsInputDisabledF(() => selected == null)
                            .OnPressF(() => NodeSR(root).Mutate(r =>
                                module.Get<ModuleMultiplayerDeleteServerMenu>().Create(r, selected!)));

                        Node(bottomRow)
                            .Mutate(s.Button)
                            .TextV("Refresh");

                        Node(bottomRow)
                            .Mutate(s.Button)
                            .TextV("Back")
                            .OnPressF(() => NodeStackPopR(root));
                    }
                }
            }
        }

        void Connect()
        {
            multiplayerConnectAction.Start(selected!.Host, selected!.Port);
            NodeSR(root).Mutate(connectingMenu.Create);
        }
    }
}
