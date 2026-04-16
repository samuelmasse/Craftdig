namespace Craftdig.Menus;

[Module]
public class ModuleMainMenu(
    RootScreen screen,
    RootKeyboard keyboard,
    RootText text,
    AppStyle s,
    AppClientOptions clientOptions,
    ModuleSingleplayerWorldSelectMenu worldSelectMenu,
    ModuleMultiplayerServerBrowserMenu serverBrowserMenu,
    ModuleMultiplayerLoginMenu loginMenu,
    ModuleMultiplayerCredentials multiplayerCredentials)
{
    public void Create(EntMut root)
    {
        Node(root, out var list)
            .Mutate(s.VerticalList)
            .AlignmentV(Alignment.Center)
            .InnerSpacingV(s.ItemSpacingXXL)
            .SizeInnerMaxRelativeV(s.Horizontal);
        {
            Node(list)
                .Mutate(s.Label)
                .TextV("Craftdig")
                .FontSizeV(s.FontSizeTitle)
                .FontPaddingV((s.ItemSpacing, 0, s.ItemSpacing, 0))
                .AlignmentV(Alignment.Horizontal);

            Node(list, out var list2)
                .Mutate(s.VerticalList)
                .AlignmentV(Alignment.Horizontal)
                .InnerSpacingV(s.ItemSpacing)
                .SizeV((s.ItemWidth, 0));
            {
                Node(list2)
                    .Mutate(s.Button)
                    .OnPressF(() => PushMenu(root, worldSelectMenu.Create))
                    .TextV("Singleplayer");

                Node(list2)
                    .Mutate(s.Button)
                    .TextV("Multiplayer")
                    .OnPressF(() =>
                    {
                        if (clientOptions.NoAuthUser == null)
                        {
                            if (!multiplayerCredentials.NeedLogin)
                            {
                                multiplayerCredentials.StartLogin();
                                multiplayerCredentials.WaitLogin();
                                PushMenu(root, serverBrowserMenu.Create);
                            }
                            else PushMenu(root, loginMenu.Create);
                        }
                        else PushMenu(root, serverBrowserMenu.Create);
                    });

                Node(list2)
                    .Mutate(s.Button)
                    .OnPressF(screen.Close)
                    .TextV("Quit");
            }
        }

        Node(root)
            .Mutate(s.Label)
            .TextV("Craftdig 0.1")
            .AlignmentV(Alignment.Left | Alignment.Bottom)
            .OffsetV((s.ItemSpacingS, -s.ItemSpacingXS));

        if (clientOptions.AllowNoAuth || clientOptions.AllowRawTcp)
        {
            Node(root)
                .Mutate(s.Label)
                .TextF(() => text.Format("UseRawTcp = {0}, NoAuthUser = {1}", clientOptions.UseRawTcp, clientOptions.NoAuthUser))
                .AlignmentV(Alignment.Right | Alignment.Bottom)
                .OffsetV((-s.ItemSpacingS, -s.ItemSpacingXS))
                .OnFrameF(() =>
                {
                    if (clientOptions.AllowRawTcp)
                    {
                        if (keyboard.IsKeyPressed(Keys.F3))
                            clientOptions.UseRawTcp = !clientOptions.UseRawTcp;
                    }

                    if (clientOptions.AllowNoAuth)
                    {
                        if (keyboard.IsKeyPressed(Keys.F4))
                        {
                            if (clientOptions.NoAuthUser == null)
                                clientOptions.NoAuthUser = clientOptions.DefaultNoAuthUser;
                            else clientOptions.NoAuthUser = null;
                        }
                    }
                });
        }
    }
}
