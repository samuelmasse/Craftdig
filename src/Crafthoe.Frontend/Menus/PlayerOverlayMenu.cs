namespace Crafthoe.Frontend;

[Player]
public class PlayerOverlayMenu(AppStyle s, PlayerEnt ent)
{
    public void Create(EntObj root)
    {
        Node(root, out var verticalList)
            .Mut(s.VerticalList)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .AlignmentV(Alignment.Bottom | Alignment.Horizontal);
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
                .Mut(s.HorizontalList)
                .PaddingV((s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS))
                .ColorV(s.BoardColor)
                .SizeInnerMaxRelativeV(s.Vertical)
                .InnerSpacingV(s.ItemSpacingS);
            {
                for (int i = 0; i < HotBarSlots.Count; i++)
                {
                    int k = i;

                    Node(bar, out var square)
                        .SizeV((s.SlotSize, s.SlotSize))
                        .SizeRelativeV((0, 0))
                        .ColorF(() => ent.Ent.HotBarIndex() == k ? (0, 0, 1, 1) : (0, 1, 0, 1));
                }
            }
        }
    }
}
