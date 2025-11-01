namespace Crafthoe.Menus.Common;

[Player]
public class PlayerCreativeInventoryMenu(ModuleEnts ents, AppStyle s, PlayerEnt player)
{
    public void Create(EntObj root)
    {
        int rows = 5;
        var blocks = new ItemSlot[(rows + 1) * HotBarSlots.Count];
        int count = 0;

        foreach (var ent in ents.Span)
        {
            if (ent.IsBlock() && ent.IsBuildable())
                blocks[count++] = new(ent, 1);
        }

        Node(root, out var vert)
            .Mut(s.VerticalList)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .PaddingV((s.ItemSpacing, s.ItemSpacing, s.ItemSpacing, s.ItemSpacing))
            .InnerSpacingV(s.ItemSpacing)
            .ColorV(s.BoardColor)
            .IsSelectableV(true)
            .AlignmentV(Alignment.Center)
            .OffsetMultiplierV(s.ItemSpacingXS);

        Node(vert, out var title)
            .Mut(s.Label)
            .TextV("Building Blocks");

        Node(vert, out var blocksVert)
            .Mut(s.VerticalList)
            .SizeInnerMaxRelativeV(s.Horizontal);
        for (int y = 0; y < rows; y++)
        {
            Node(blocksVert, out var blocksHor)
                .Mut(s.HorizontalList)
                .SizeInnerMaxRelativeV(s.Vertical);

            for (int x = 0; x < HotBarSlots.Count; x++)
            {
                Vector2i loc = (x, y);

                Node(blocksHor, out var square)
                    .Mut(s.Button)
                    .Mut(s.Slot)
                    .PlayerV((EntMut)player.Ent)
                    .GetSlotValueF(() => blocks[loc.Y * HotBarSlots.Count + loc.X]);
                {
                    Node(square)
                        .Mut(s.SlotButtonInfinity)
                        .Mut(s.SlotTooltip)
                        .SlotV(square);
                }
            }
        }

        Node(vert, out var hotbar)
            .Mut(s.HorizontalList)
            .SizeInnerMaxRelativeV(s.Vertical);
        for (int x = 0; x < HotBarSlots.Count; x++)
        {
            int i = x;

            Node(hotbar, out var square)
                .Mut(s.Button)
                .Mut(s.Slot)
                .GetSlotValueF(() => player.Ent.HotBarSlots()[i])
                .SetSlotValueF((v) => player.Ent.HotBarSlots()[i] = v)
                .PlayerV((EntMut)player.Ent);
            {
                Node(square)
                    .Mut(s.SlotButton)
                    .Mut(s.SlotTooltip)
                    .SlotV(square);
            }
        }
    }
}
