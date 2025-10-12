namespace Crafthoe.Frontend;

[App]
public class AppSinglePlayerNewWorldMenu(AppStyle s, AppLoadWorldAction loadWorldAction)
{
    public void Create(EntObj root)
    {
        string name = "New World";

        Node(root, out var form)
            .Mut(s.VerticalList)
            .OffsetV((0, s.ItemHeight))
            .SizeV((s.ItemWidth * 2, 0))
            .InnerSpacingV(s.ItemSpacing)
            .AlignmentV(Alignment.Horizontal);
        {
            Node(form)
                .Mut(s.Label)
                .TextV("World Name");

            Node(form, out var nameTxt)
                .Mut(s.Label)
                .TextF(() => name);

            Node(form)
                .Mut(s.Button)
                .TextV("Game Mode: Survival");

            Node(form)
                .Mut(s.Button)
                .TextV("Difficulty: Normal");
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
                    Node(leftButtonsVertical)
                        .OnPressF(loadWorldAction.Run)
                        .TextV("Create New World")
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
