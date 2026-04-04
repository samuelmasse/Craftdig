namespace Craftdig.Menus.Common;

[Player]
public class PlayerOverlayMenu(AppStyle s, PlayerEnt ent)
{
    public void Create(EntMut root)
    {
        Node(root, out var verticalList)
            .Mutate(s.VerticalList)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .AlignmentV(Alignment.Bottom | Alignment.Horizontal);
        {
            var sw = Stopwatch.StartNew();
            Ent lastSelected = default;

            Node(verticalList, out var itemTooltip)
                .Mutate(s.Label)
                .AlignmentV(Alignment.Horizontal)
                .SizeTextRelativeV(s.Horizontal + s.Vertical * 2)
                .TextF(() =>
                {
                    var selected = ent.HotBarSlots[ent.HotBarIndex];

                    if (lastSelected != selected.Item)
                    {
                        lastSelected = selected.Item;
                        sw.Restart();
                    }

                    return selected.Item.Name ?? string.Empty;
                })
                .TextColorF(() => s.TextColor * (1, 1, 1, Math.Clamp(3 - (float)sw.Elapsed.TotalSeconds * 4, 0, 1)));

            Node(verticalList, out var barContainer)
                .SizeInnerMaxRelativeV(s.Vertical + s.Horizontal);
            {
                Node(barContainer, out var bar)
                    .Mutate(s.HorizontalList)
                    .PaddingV((s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS))
                    .ColorV(s.BoardColor)
                    .SizeInnerMaxRelativeV(s.Vertical);
                for (int i = 0; i < Slots.HotBarCount; i++)
                {
                    int k = i;

                    Node(bar, out var square)
                        .Mutate(s.Slot)
                        .GetSlotValueF(() => ent.HotBarSlots[k])
                        .ColorV((0, 1, 0, 1));
                }

                Node(barContainer, out var puck)
                    .SizeV((s.SlotSize + s.ItemSpacingS * 2, s.SlotSize + s.ItemSpacingS * 2))
                    .OffsetF(() => (ent.HotBarIndex * s.SlotSize, 0));
                {
                    Node(puck, out var puckTop)
                        .SizeRelativeV(s.Horizontal)
                        .SizeV((0, s.ItemSpacingS))
                        .ColorV((0, 0, 1, 1));

                    Node(puck, out var puckBottom)
                        .AlignmentV(Alignment.Bottom)
                        .SizeRelativeV(s.Horizontal)
                        .SizeV((0, s.ItemSpacingS))
                        .ColorV((0, 0, 1, 1));

                    Node(puck, out var puckLeft)
                        .SizeRelativeV(s.Vertical)
                        .SizeV((s.ItemSpacingS, 0))
                        .ColorV((0, 0, 1, 1));

                    Node(puck, out var puckRight)
                        .AlignmentV(Alignment.Right)
                        .SizeRelativeV(s.Vertical)
                        .SizeV((s.ItemSpacingS, 0))
                        .ColorV((0, 0, 1, 1));
                }
            }
        }
    }
}
