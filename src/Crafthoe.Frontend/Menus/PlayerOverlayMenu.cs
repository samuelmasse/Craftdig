namespace Crafthoe.Frontend;

[Player]
public class PlayerOverlayMenu(AppStyle s, PlayerEnt ent)
{
    public void Create(EntObj root)
    {
        Node(root, out var verticalList)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .SizeInnerSumRelativeV(s.Vertical)
            .AlignmentV(Alignment.Bottom | Alignment.Horizontal)
            .InnerLayoutV(InnerLayout.VerticalList);
        {
            var sw = Stopwatch.StartNew();
            Ent lastSelected = default;

            Node(verticalList, out var itemTooltip)
                .Mut(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .SizeTextRelativeV(s.Horizontal + s.Vertical * 2)
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
                .TextColorF(() => s.TextColor * (1, 1, 1, Math.Clamp(3 - (float)sw.Elapsed.TotalSeconds * 4, 0, 1)));

            Node(verticalList, out var bar)
                .PaddingV((s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS))
                .ColorV(s.BoardColor)
                .SizeInnerMaxRelativeV(s.Vertical)
                .SizeInnerSumRelativeV(s.Horizontal)
                .InnerLayoutV(InnerLayout.HorizontalList)
                .InnerSpacingV(s.ItemSpacingS);
            {
                for (int i = 0; i < HotBarSlots.Count; i++)
                {
                    int k = i;

                    Node(bar, out var square)
                        .SizeV((s.SlotSize, s.SlotSize))
                        .ColorF(() => ent.Ent.HotBarIndex() == k ? (0, 0, 1, 1) : (0, 1, 0, 1));
                }
            }
        }
    }
}
