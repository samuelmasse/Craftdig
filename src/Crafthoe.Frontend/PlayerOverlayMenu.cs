namespace Crafthoe.Frontend;

[Player]
public class PlayerOverlayMenu(AppStyle s, PlayerEnt ent)
{
    public EntObj Get()
    {
        Node(out var menu).SizeRelativeV((1, 1));

        Node(menu, out var verticalList)
            .SizeInnerMaxRelativeV((1, 0))
            .SizeInnerSumRelativeV((0, 1))
            .AlignmentV(Alignment.Bottom | Alignment.Horizontal)
            .InnerLayoutV(InnerLayout.VerticalList);
        {
            var sw = Stopwatch.StartNew();
            Ent lastSelected = default;

            Node(verticalList, out var itemTooltip)
                .Mut(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .SizeTextRelativeV((1, 2))
                .TextF(() =>
                {
                    var selected = ent.Ent.HotBarSlots()[ent.Ent.HotBarIndex()];

                    if (lastSelected != selected)
                    {
                        lastSelected = selected;
                        sw.Restart();
                    }

                    return selected.Name() ?? string.Empty;
                })
                .TextColorF(() => (1, 1, 1, Math.Clamp(3 - (float)sw.Elapsed.TotalSeconds * 4, 0, 1)));

            Node(verticalList, out var bar)
                .PaddingV((16, 16, 16, 16))
                .ColorV((1, 0, 0, 1))
                .SizeInnerMaxRelativeV((0, 1))
                .SizeInnerSumRelativeV((1, 0))
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSpacingV(16);
            {
                for (int i = 0; i < HotBarSlots.Count; i++)
                {
                    int k = i;

                    Node(bar, out var square)
                        .SizeV((120, 120))
                        .ColorF(() => ent.Ent.HotBarIndex() == k ? (0, 0, 1, 1) : (0, 1, 0, 1));
                }
            }
        }

        return menu;
    }
}
