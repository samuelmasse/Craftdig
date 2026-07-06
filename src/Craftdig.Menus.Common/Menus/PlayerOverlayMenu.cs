namespace Craftdig.Menus.Common;

[Player]
public class PlayerOverlayMenu(AppStyle s, PlayerEnt ent)
{
    public void Create(EntMut root)
    {
        Node(root, out var bar)
            .Mutate(s.HorizontalList)
            .PaddingV((s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS, s.ItemSpacingS))
            .ColorV(s.BoardColor)
            .OffsetV((0, s.ItemSpacingXS))
            .SizeInnerMaxRelativeV(s.Vertical)
            .AlignmentV(Alignment.Bottom | Alignment.Horizontal);
        for (int i = 0; i < Slots.HotBarCount; i++)
        {
            int k = i;

            Node(bar, out var square)
                .Mutate(s.Slot)
                .GetSlotValueF(() => ent.HotBarSlots[k]);
        }

        Node(root, out var puck)
            .SizeV((s.SlotSize + s.ItemSpacingS * 2, s.SlotSize + s.ItemSpacingS * 2))
            .SizeRelativeV((0, 0))
            .OffsetF(() => bar.OffsetR + (ent.HotBarIndex * s.SlotSize, 0));
        {
            var color = new Vec4(0.35f, 0.35f, 0.35f, 1);

            Node(puck, out var puckTop)
                .SizeRelativeV(s.Horizontal)
                .SizeV((0, s.ItemSpacingS))
                .ColorV(color);

            Node(puck, out var puckBottom)
                .AlignmentV(Alignment.Bottom)
                .SizeRelativeV(s.Horizontal)
                .SizeV((0, s.ItemSpacingS))
                .ColorV(color);

            Node(puck, out var puckLeft)
                .SizeRelativeV(s.Vertical)
                .SizeV((s.ItemSpacingS, 0))
                .ColorV(color);

            Node(puck, out var puckRight)
                .AlignmentV(Alignment.Right)
                .SizeRelativeV(s.Vertical)
                .SizeV((s.ItemSpacingS, 0))
                .ColorV(color);
        }

        var sw = Stopwatch.StartNew();
        Ent lastSelected = default;
        Vec4 Multiplier() => (1, 1, 1, Math.Clamp(3 - (float)sw.Elapsed.TotalSeconds * 4, 0, 1));

        Node(root, out var itemTooltip)
            .Mutate(s.Label)
            .AlignmentV(Alignment.Horizontal)
            .OffsetF(() => (0, bar.OffsetR.Y - s.ItemHeight))
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
            .TextColorF(() => s.TextColor * Multiplier())
            .TextShadowColorF(() => s.TextShadowColor * Multiplier());
    }
}
