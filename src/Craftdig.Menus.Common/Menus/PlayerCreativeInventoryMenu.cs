namespace Craftdig.Menus.Common;

[Player]
public class PlayerCreativeInventoryMenu(ModuleEnts ents, AppStyle s, PlayerEnt player)
{
    public void Create(EntMut root)
    {
        int rows = 5;
        var blocks = new ItemSlot[(rows + 1) * Slots.HotBarCount];
        int count = 0;

        foreach (var ent in ents.Span)
        {
            if (ent.IsBlock && ent.IsBuildable)
                blocks[count++] = new(ent, 1);
        }

        Node(root, out var vert)
            .Mutate(s.VerticalList)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .PaddingV((s.ItemSpacing, s.ItemSpacing, s.ItemSpacing, s.ItemSpacing))
            .InnerSpacingV(s.ItemSpacing)
            .ColorV(s.BoardColor)
            .IsSelectableV(true)
            .AlignmentV(Alignment.Center)
            .OffsetMultiplierV(s.ItemSpacingXS);

        Node(vert, out var title)
            .Mutate(s.Label)
            .TextV("Building Blocks");

        Node(vert, out var blocksVert)
            .Mutate(s.VerticalList)
            .SizeInnerMaxRelativeV(s.Horizontal);
        for (int y = 0; y < rows; y++)
        {
            Node(blocksVert, out var blocksHor)
                .Mutate(s.HorizontalList)
                .SizeInnerMaxRelativeV(s.Vertical);

            for (int x = 0; x < Slots.HotBarCount; x++)
            {
                Vector2i loc = (x, y);

                Node(blocksHor, out var square)
                    .Mutate(s.Button)
                    .Mutate(s.Slot)
                    .PlayerV(player)
                    .GetSlotValueF(() => blocks[loc.Y * Slots.HotBarCount + loc.X]);
                {
                    Node(square)
                        .Mutate(s.SlotButtonInfinity)
                        .Mutate(s.SlotTooltip)
                        .SlotV(square);
                }
            }
        }

        Node(vert, out var hotbar)
            .Mutate(s.HorizontalList)
            .SizeInnerMaxRelativeV(s.Vertical);
        for (int x = 0; x < Slots.HotBarCount; x++)
        {
            int i = x;

            Node(hotbar, out var square)
                .Mutate(s.Button)
                .Mutate(s.Slot)
                .GetSlotValueF(() => player.HotBarSlots[i])
                .SetSlotValueF((v) => player.HotBarSlots[i] = v)
                .PlayerV(player);
            {
                Node(square)
                    .Mutate(s.SlotButton)
                    .Mutate(s.SlotTooltip)
                    .SlotV(square);
            }
        }
    }
}
