namespace Crafthoe.Frontend;

[App]
public class AppMainMenu(
    RootScreen screen,
    AppStyle s,
    AppSinglePlayerWorldSelectMenu worldSelectMenu)
{
    public void Create(EntObj root)
    {
        Node(root, out var list)
            .AlignmentV(Alignment.Center)
            .InnerLayoutV(InnerLayout.VerticalList)
            .InnerSpacingV(s.ItemSpacingXXL)
            .SizeInnerSumRelativeV(s.Vertical)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .ColorV(s.BoardColor);
        {
            Node(list)
                .Mut(s.Label)
                .TextV("Crafthoe")
                .FontSizeV(s.FontSizeTitle)
                .ColorV(s.ButtonColor)
                .AlignmentV(Alignment.Horizontal);

            Node(list, out var list2)
                .AlignmentV(Alignment.Horizontal)
                .InnerLayoutV(InnerLayout.VerticalList)
                .InnerSpacingV(s.ItemSpacing)
                .SizeV((s.ItemWidth, 0))
                .SizeInnerSumRelativeV(s.Vertical)
                .ColorV(s.BoardColor2);
            {
                Node(list2)
                    .Mut(s.Button)
                    .OnPressF(() => root.StackRootV().NodeStack().Push(
                        Node()
                            .SizeRelativeV((1, 1))
                            .StackRootV(root.StackRootV())
                            .Mut(worldSelectMenu.Create)))
                    .TextV("Singleplayer");

                Node(list2)
                    .Mut(s.Button)
                    .TextV("Multiplayer");

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
