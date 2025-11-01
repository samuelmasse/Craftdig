namespace Crafthoe.Menus.Common;

[Player]
public class PlayerEscapeMenu(AppReset reset, AppStyle s, ModuleScope scope)
{
    public void Create(EntObj root)
    {
        Node(root, out var list)
            .Mut(s.VerticalList)
            .SizeV((s.ItemWidth, 0))
            .AlignmentV(Alignment.Horizontal)
            .InnerSpacingV(s.ItemSpacing)
            .PaddingV((s.ItemSpacing, s.ItemSpacing, s.ItemSpacing, s.ItemSpacing))
            .AlignmentV(Alignment.Center)
            .OffsetMultiplierV(s.ItemSpacingXS)
            .ColorV(s.BoardColor);
        {
            Node(list)
                .Mut(s.Label)
                .TextV("Game Menu")
                .AlignmentV(Alignment.Horizontal);

            Node(list)
                .Mut(s.Button)
                .OnPressF(() => root.StackRootV()?.NodeStack().Pop())
                .TextV("Back to Game");

            Node(list)
                .Mut(s.Button)
                .OnPressF(reset.Run)
                .TextV("Quit");
        }
    }
}
