namespace Crafthoe.Menus;

[Module]
public class ModuleMainMenu(
    RootScreen screen,
    AppStyle s,
    ModuleSingleplayerWorldSelectMenu worldSelectMenu,
    ModuleMultiplayerConnectMenu connectMenu,
    ModuleMultiplayerLoginMenu loginMenu,
    ModuleMultiplayerCredentials multiplayerCredentials)
{
    public void Create(EntObj root)
    {
        Node(root, out var list)
            .Mut(s.VerticalList)
            .AlignmentV(Alignment.Center)
            .OffsetMultiplierV(s.ItemSpacingXS)
            .InnerSpacingV(s.ItemSpacingXXL)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .ColorV(s.BoardColor);
        {
            Node(list)
                .Mut(s.Label)
                .TextV("Crafthoe")
                .FontSizeV(s.FontSizeTitle)
                .FontPaddingV((s.ItemSpacing, 0, s.ItemSpacing, 0))
                .ColorV(s.ButtonColor)
                .AlignmentV(Alignment.Horizontal);

            Node(list, out var list2)
                .Mut(s.VerticalList)
                .AlignmentV(Alignment.Horizontal)
                .InnerSpacingV(s.ItemSpacing)
                .SizeV((s.ItemWidth, 0))
                .ColorV(s.BoardColor2);
            {
                Node(list2)
                    .Mut(s.Button)
                    .OnPressF(() => root.StackRootV()?.NodeStack().Push(
                        Node().StackRootV(root.StackRootV()).Mut(worldSelectMenu.Create)))
                    .TextV("Singleplayer");

                Node(list2)
                    .Mut(s.Button)
                    .TextV("Multiplayer")
                    .OnPressF(() =>
                    {
                        var node = Node().StackRootV(root.StackRootV());

                        if (!multiplayerCredentials.NeedLogin)
                        {
                            multiplayerCredentials.StartLogin();
                            multiplayerCredentials.WaitLogin();
                            node.Mut(connectMenu.Create);
                        }
                        else node.Mut(loginMenu.Create);

                        root.StackRootV()?.NodeStack().Push(node);
                    });

                Node(list2)
                    .Mut(s.Button)
                    .OnPressF(screen.Close)
                    .TextV("Quit");
            }
        }

        Node(root)
            .Mut(s.Label)
            .TextV("Crafthoe 0.1")
            .AlignmentV(Alignment.Left | Alignment.Bottom)
            .OffsetV((s.ItemSpacingS, -s.ItemSpacingXS));
    }
}
