namespace Crafthoe.Frontend;

[App]
public class AppMainMenu(
    RootScreen screen,
    AppStyle s,
    AppSinglePlayerWorldSelectMenu singlePlayerWorldSelectMenu)
{
    public EntObj Get(EntObj ui)
    {
        Node(out var menu).SizeRelativeV((1, 1));

        Node(menu, out var list)
            .AlignmentV(Alignment.Center)
            .InnerLayoutV(InnerLayout.VerticalList)
            .InnerSpacingV(80)
            .SizeInnerSumRelativeV((0, 1))
            .SizeInnerMaxRelativeV((1, 0))
            .ColorV((1, 0, 0, 1));
        {
            Node(list)
                .Mut(s.Label)
                .TextV("Crafthoe")
                .FontSizeV(300)
                .ColorV((1, 0, 1, 1))
                .AlignmentV(Alignment.Horizontal);

            Node(list, out var list2)
                .AlignmentV(Alignment.Horizontal)
                .InnerLayoutV(InnerLayout.VerticalList)
                .InnerSpacingV(40)
                .SizeV((512, 0))
                .SizeInnerSumRelativeV((0, 1))
                .ColorV((1, 1, 0, 1));
            {
                Node(list2)
                    .Mut(s.Button)
                    .OnPressF(() => ui.NodeStack().Push(singlePlayerWorldSelectMenu.Get(ui)))
                    .TextV("Singleplayer");

                Node(list2)
                    .Mut(s.Button)
                    .TextV("Multiplayer");

                Node(list2, out var list3)
                    .Mut(s.Button)
                    .OnPressF(screen.Close)
                    .TextV("Quit");
            }
        }

        Node(menu)
            .Mut(s.Label)
            .TextV("Crafthoe 0.1")
            .AlignmentV(Alignment.Left | Alignment.Bottom)
            .OffsetV((15, -10));

        return menu;
    }
}
