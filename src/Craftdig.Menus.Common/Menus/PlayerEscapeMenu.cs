namespace Craftdig.Menus.Common;

[Player]
public class PlayerEscapeMenu(AppReset reset, AppStyle s)
{
    public void Create(EntMut root)
    {
        Node(root, out var list)
            .Mutate(s.VerticalList)
            .SizeV((s.ItemWidthL, 0))
            .AlignmentV(Alignment.Horizontal)
            .InnerSpacingV(s.ItemSpacing)
            .PaddingV((s.ItemSpacing, s.ItemSpacing, s.ItemSpacing, s.ItemSpacing))
            .AlignmentV(Alignment.Center)
            .ColorV(s.BoardColor);
        {
            Node(list)
                .Mutate(s.LabelDark)
                .TextV("Game Menu")
                .AlignmentV(Alignment.Horizontal);

            Node(list)
                .Mutate(s.Button)
                .OnPressF(() => PopMenu(root))
                .TextV("Back to Game");

            Node(list)
                .Mutate(s.Button)
                .OnPressF(reset.Run)
                .TextV("Quit");
        }
    }
}
