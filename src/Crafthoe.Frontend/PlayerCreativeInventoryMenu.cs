namespace Crafthoe.Frontend;

[Player]
public class PlayerCreativeInventoryMenu(AppStyle s)
{
    public EntObj Get()
    {
        Node(out var menu).SizeRelativeV((1, 1));

        Node(menu, out var vert)
            .SizeInnerSumRelativeV((0, 1))
            .SizeInnerMaxRelativeV((1, 0))
            .PaddingV((16, 16, 16, 16))
            .InnerSpacingV(16)
            .ColorV((1, 0, 0, 1))
            .IsSelectableV(true)
            .InnerLayoutV(InnerLayout.VerticalList)
            .AlignmentV(Alignment.Center);
        {
            for (int y = 0; y < 5; y++)
            {
                Node(vert, out var hor)
                    .SizeInnerSumRelativeV((1, 0))
                    .SizeInnerMaxRelativeV((0, 1))
                    .InnerSpacingV(16)
                    .InnerLayoutV(InnerLayout.HorizontalList);
                {
                    for (int x = 0; x < HotBarSlots.Count; x++)
                    {
                        Node(hor, out var square)
                            .Mut(s.Button)
                            .SizeV((120, 120));
                    }
                }
            }
        }

        return menu;
    }
}
