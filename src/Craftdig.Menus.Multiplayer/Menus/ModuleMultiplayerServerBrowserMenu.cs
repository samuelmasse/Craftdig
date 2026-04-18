namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerServerBrowserMenu(
    RootText text,
    AppStyle s,
    AppClientOptions clientOptions,
    ModuleScope module,
    ModuleMultiplayerServerList serverList,
    ModuleMultiplayerCredentials multiplayerCredentials,
    ModuleMultiplayerConnectAction multiplayerConnectAction,
    ModuleMultiplayerServerCache serverCache,
    ModuleMultiplayerConnectMenu connectMenu,
    ModuleMultiplayerConnectingMenu connectingMenu)
{
    public void Create(EntMut root)
    {
        ServerEntry? selected = null;
        StringBuilder? user = clientOptions.NoAuthUser != null ? new(clientOptions.NoAuthUser) : null;

        var serverIcons = module.New<ModuleMultiplayerServerIcons>();
        var serverPinger = module.New<ModuleMultiplayerServerPinger>();
        serverCache.Prune(serverList.Servers);
        serverPinger.PingAll(serverList.Servers);

        Node(root, out var topBar)
            .Mutate(s.TopBar);
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
                        .TextboxStringBuilderV(user)
                        .TextboxOnTextUpdatedF(() =>
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
                        PopMenu(root);
                        PushMenu(root, module.Get<ModuleMultiplayerLoginMenu>().Create);
                    });
            }
        }

        Node(root, out var middle)
            .Mutate(s.MiddleBar);
        {
            s.Selector(middle, out var select);

            for (int i = 0; i < serverList.Servers.Length; i++)
            {
                var server = serverList.Servers[i];
                var itemHeight = 128;
                var cachedDesc = serverCache.LoadDescription(server.Address);
                var cachedIcon = serverCache.LoadIcon(server.Address);

                Node(select, out var item)
                    .Mutate(s.SelectorItem)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.ItemWidthL * 2f, itemHeight + s.ItemSpacingS * 2))
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
                            .ColorV((0.2f, 0, 0.6f, 1))
                            .TextureF(() =>
                            {
                                var result = serverPinger[server.Address];
                                var data = result?.IconData ?? cachedIcon;
                                return data != null ? serverIcons[data] : null;
                            });

                        var start = DateTime.UtcNow;
                        var speed = TimeSpan.FromMilliseconds(150);
                        var wait = speed * 2;

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
                                .TextF(() =>
                                {
                                    var result = serverPinger[server.Address];
                                    var desc = result?.Description ?? cachedDesc;

                                    if (result is { Success: false })
                                        return "Can't connect to server";

                                    if (desc != null)
                                        return text.Format("{0}", desc);

                                    return "A Craftdig Server";
                                })
                                .TextColorF(() =>
                                {
                                    var result = serverPinger[server.Address];
                                    if (result?.Success == false)
                                        return (1, 0, 0, 1);

                                    return s.TextColorFaint;
                                });
                        }

                        Node(itemContainer, out var rightInfo)
                            .Mutate(s.VerticalList)
                            .SizeInnerMaxRelativeV((1, 0))
                            .AlignmentV(Alignment.Right | Alignment.Vertical)
                            .OffsetV((-s.ItemSpacing, 0));
                        {
                            Node(rightInfo)
                                .Mutate(s.Label)
                                .AlignmentV(Alignment.Right)
                                .TextF(() =>
                                {
                                    var result = serverPinger[server.Address];
                                    var elapsed = DateTime.UtcNow - start;

                                    if (result == null || elapsed < wait)
                                    {
                                        var dots = (int)(elapsed / speed) % 3 + 1;
                                        string Dot(int i) => dots >= i ? "." : "";
                                        return text.Format("{0}{1}{2}", Dot(1), Dot(2), Dot(3));
                                    }

                                    if (result.Ping.HasValue)
                                        return text.Format("{0:0}ms", result.Ping.Value.TotalMilliseconds);

                                    return "x";
                                })
                                .TextColorF(() =>
                                {
                                    var result = serverPinger[server.Address];
                                    var elapsed = DateTime.UtcNow - start;

                                    if (result == null || elapsed < wait)
                                        return (0, 1, 1, 1);

                                    return result.Success ? (0, 1, 0, 1) : (1, 0, 0, 1);
                                });

                            Node(rightInfo)
                                .Mutate(s.Label)
                                .AlignmentV(Alignment.Right)
                                .TextColorV(s.TextColorFaint)
                                .TextF(() =>
                                {
                                    var result = serverPinger[server.Address];
                                    var elapsed = DateTime.UtcNow - start;

                                    if (result?.CurrentPlayers != null && result?.MaxPlayers != null && elapsed >= wait)
                                        return text.Format("{0}/{1}", result.CurrentPlayers, result.MaxPlayers);

                                    return string.Empty;
                                });
                        }
                    }
                }
            }
        }

        Node(root, out var bottomBar)
            .Mutate(s.BottomBar);
        {
            Node(bottomBar, out var buttonsList)
                .Mutate(s.ButtonBar);
            {
                Node(buttonsList, out var leftButtons)
                    .Mutate(s.VerticalList)
                    .SizeV((s.ItemWidthL * 2.5f, 0))
                    .InnerSpacingV(s.ItemSpacing);
                {
                    Node(leftButtons, out var topRow)
                        .Mutate(s.ButtonRow);
                    {
                        Node(topRow)
                            .Mutate(s.Button)
                            .TextV("Join Server")
                            .IsInputDisabledF(() => selected == null)
                            .OnPressF(Connect);

                        Node(topRow)
                            .Mutate(s.Button)
                            .TextV("Direct Connect")
                            .OnPressF(() => PushMenu(root, connectMenu.Create));

                        Node(topRow)
                            .Mutate(s.Button)
                            .TextV("Add Server")
                            .OnPressF(() => PushMenu(root, r =>
                                module.Get<ModuleMultiplayerAddServerMenu>().Create(r, null)));
                    }

                    Node(leftButtons, out var bottomRow)
                        .Mutate(s.ButtonRow);
                    {
                        Node(bottomRow)
                            .Mutate(s.Button)
                            .TextV("Edit")
                            .IsInputDisabledF(() => selected == null)
                            .OnPressF(() => PushMenu(root, r =>
                                module.Get<ModuleMultiplayerAddServerMenu>().Create(r, selected)));

                        Node(bottomRow)
                            .Mutate(s.Button)
                            .TextV("Delete")
                            .IsInputDisabledF(() => selected == null)
                            .OnPressF(() => PushMenu(root, r =>
                                module.Get<ModuleMultiplayerDeleteServerMenu>().Create(r, selected!)));

                        Node(bottomRow)
                            .Mutate(s.Button)
                            .TextV("Refresh")
                            .OnPressF(() => RefreshMenu(root));

                        Node(bottomRow)
                            .Mutate(s.Button)
                            .TextV("Back")
                            .OnPressF(() => PopMenu(root));
                    }
                }
            }
        }

        void Connect()
        {
            multiplayerConnectAction.Start(selected!.Address);
            PushMenu(root, connectingMenu.Create);
        }
    }
}
