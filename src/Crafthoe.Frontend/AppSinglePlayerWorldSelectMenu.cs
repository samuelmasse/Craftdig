namespace Crafthoe.Frontend;

[App]
public class AppSinglePlayerWorldSelectMenu(AppStyle s, AppLoadWorldAction loadWorldAction)
{
    public EntObj Get(EntObj ui)
    {
        Node(out var menu).SizeRelativeV((1, 1));

        Node(menu, out var topBar)
            .SizeRelativeV((1, 0))
            .SizeV((0, 300))
            .ColorV((1, 0, 0, 1));

        Node(menu, out var bottomBar)
            .SizeRelativeV((1, 0))
            .SizeV((0, 300))
            .AlignmentV(Alignment.Horizontal | Alignment.Bottom)
            .ColorV((1, 0, 0, 1));
        {
            Node(bottomBar, out var buttonsList)
                .AlignmentV(Alignment.Center)
                .SizeInnerSumRelativeV((1, 0))
                .SizeInnerMaxRelativeV((0, 1))
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSpacingV(40)
                .ColorV((0.5f, 0.7f, 0, 1));
            {
                Node(buttonsList, out var leftButtonsVertical)
                    .SizeV((1024, 0))
                    .SizeInnerSumRelativeV((0, 1))
                    .InnerSpacingV(30)
                    .InnerLayoutV(InnerLayout.VerticalList);
                {
                    Node(leftButtonsVertical)
                        .TextV("Play Selected World")
                        .Mut(s.Button);

                    Node(leftButtonsVertical, out var leftButtonsHorizontal)
                        .SizeRelativeV((1, 0))
                        .SizeInnerMaxRelativeV((0, 1))
                        .InnerSpacingV(30)
                        .InnerLayoutV(InnerLayout.HorizontalList)
                        .InnerSizingV(InnerSizing.HorizontalWeight);
                    {
                        Node(leftButtonsHorizontal)
                            .TextV("Edit")
                            .Mut(s.Button);

                        Node(leftButtonsHorizontal)
                            .TextV("Delete")
                            .Mut(s.Button);
                    }
                }

                Node(buttonsList, out var rightButtonsVertical)
                    .SizeV((1024, 0))
                    .SizeInnerSumRelativeV((0, 1))
                    .InnerSpacingV(30)
                    .InnerLayoutV(InnerLayout.VerticalList);
                {
                    Node(rightButtonsVertical)
                        .OnPressF(loadWorldAction.Run)
                        .TextV("Create New World")
                        .Mut(s.Button);

                    Node(rightButtonsVertical, out var rightButtonsHorizontal)
                        .SizeRelativeV((1, 0))
                        .SizeInnerMaxRelativeV((0, 1))
                        .InnerSpacingV(30)
                        .InnerLayoutV(InnerLayout.HorizontalList)
                        .InnerSizingV(InnerSizing.HorizontalWeight);
                    {
                        Node(rightButtonsHorizontal)
                            .TextV("Re-Create")
                            .Mut(s.Button);

                        Node(rightButtonsHorizontal)
                            .OnPressF(() => ui.NodeStack().Pop())
                            .TextV("Back")
                            .Mut(s.Button);
                    }
                }
            }
        }

        return menu;
    }
}
