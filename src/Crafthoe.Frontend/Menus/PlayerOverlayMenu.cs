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

            Node(verticalList, out var barContainer)
                .SizeInnerMaxRelativeV(s.Vertical + s.Horizontal);
            {
                Node(barContainer, out var bar)
                    .Mut(s.HorizontalList)
                    .PaddingV((s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS))
                    .ColorV(s.BoardColor)
                    .SizeInnerMaxRelativeV(s.Vertical)
                    .InnerSpacingV(s.ItemSpacingS);
                for (int i = 0; i < HotBarSlots.Count; i++)
                {
                    int k = i;

                    Node(bar, out var square)
                        .Mut(s.Slot)
                        .GetSlotValueF(() => ent.Ent.HotBarSlots()[k])
                        .ColorV((0, 1, 0, 1));
                }

                Node(barContainer, out var puck)
                    .SizeV((s.SlotSize + s.ItemSpacingS * 2, s.SlotSize + s.ItemSpacingS * 2))
                    .OffsetF(() => (ent.Ent.HotBarIndex() * (s.SlotSize + s.ItemSpacingS), 0));
                {
                    Node(puck, out var puckTop)
                        .SizeRelativeV(s.Horizontal)
                        .SizeV((0, s.ItemSpacingS))
                        .ColorV((1, 1, 1, 1));

                    Node(puck, out var puckBottom)
                        .AlignmentV(Alignment.Bottom)
                        .SizeRelativeV(s.Horizontal)
                        .SizeV((0, s.ItemSpacingS))
                        .ColorV((1, 1, 1, 1));

                    Node(puck, out var puckLeft)
                        .SizeRelativeV(s.Vertical)
                        .SizeV((s.ItemSpacingS, 0))
                        .ColorV((1, 1, 1, 1));

                    Node(puck, out var puckRight)
                        .AlignmentV(Alignment.Right)
                        .SizeRelativeV(s.Vertical)
                        .SizeV((s.ItemSpacingS, 0))
                        .ColorV((1, 1, 1, 1));
                }
            }
        }
    }
}
