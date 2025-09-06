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
            .PaddingV((32, 32, 32, 32))
            .InnerSpacingV(32)
            .ColorV((1, 0, 0, 1))
            .IsSelectableV(true)
            .InnerLayoutV(InnerLayout.VerticalList)
            .AlignmentV(Alignment.Center);

        Node(vert, out var title)
            .Mut(s.Label)
            .TextV("Building Blocks");

        Node(vert, out var blocksVert)
            .SizeInnerSumRelativeV((0, 1))
            .SizeInnerMaxRelativeV((1, 0))
            .InnerSpacingV(12)
            .InnerLayoutV(InnerLayout.VerticalList);
        {
            for (int y = 0; y < 5; y++)
            {
                Node(blocksVert, out var blocksHor)
                    .SizeInnerSumRelativeV((1, 0))
                    .SizeInnerMaxRelativeV((0, 1))
                    .InnerSpacingV(12)
                    .InnerLayoutV(InnerLayout.HorizontalList);
                {
                    for (int x = 0; x < HotBarSlots.Count; x++)
                    {
                        Node(blocksHor, out var square)
                            .Mut(s.Button)
                            .SizeV((120, 120));
                    }
                }
            }
        }

        Node(vert, out var hotbar)
            .SizeInnerSumRelativeV((1, 0))
            .SizeInnerMaxRelativeV((0, 1))
            .InnerSpacingV(12)
            .InnerLayoutV(InnerLayout.HorizontalList);
        {
            for (int x = 0; x < HotBarSlots.Count; x++)
            {
                Node(hotbar, out var square)
                    .Mut(s.Button)
                    .SizeV((120, 120));
            }
        }

        return menu;
    }
}
