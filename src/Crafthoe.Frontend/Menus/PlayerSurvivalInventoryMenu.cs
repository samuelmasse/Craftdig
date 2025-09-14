namespace Crafthoe.Frontend;

[Player]
public class PlayerSurvivalInventoryMenu(AppStyle s, PlayerEnt player)
{
    public void Create(EntObj root)
    {
        Node(root, out var vert)
            .Mut(s.VerticalList)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .PaddingV((s.ItemSpacing, s.ItemSpacing, s.ItemSpacing, s.ItemSpacing))
            .InnerSpacingV(s.ItemSpacing)
            .ColorV(s.BoardColor)
            .IsSelectableV(true)
            .AlignmentV(Alignment.Center);

        Node(vert, out var playerHor)
            .Mut(s.HorizontalList)
            .InnerSpacingV(s.ItemSpacingS)
            .SizeInnerMaxRelativeV(s.Vertical);
        {
            Node(playerHor, out var armorVert)
                .Mut(s.VerticalList)
                .SizeInnerMaxRelativeV(s.Horizontal)
                .InnerSpacingV(s.ItemSpacingS);
            for (int i = 0; i < ArmorSlots.Count; i++)
            {
                int x = i;

                Node(armorVert, out var square)
                    .Mut(s.Button)
                    .Mut(s.Slot)
                    .PlayerV((EntMut)player.Ent)
                    .GetSlotValueF(() => player.Ent.ArmorSlots()[x])
                    .SetSlotValueF((v) => player.Ent.ArmorSlots()[x] = v);
                {
                    Node(square)
                        .Mut(s.SlotButton)
                        .Mut(s.SlotTooltip)
                        .SlotV(square);
                }
            }

            Node(playerHor, out var playerDisplay)
                .ColorV((0, 0, 0, 1))
                .SizeV((s.SlotSize * 3, 0))
                .SizeRelativeV((0, 1))
                .IsPostSizedV(true);

            Node(playerHor, out var craftingVert)
                .Mut(s.VerticalList)
                .SizeInnerMaxRelativeV(s.Horizontal)
                .InnerSpacingV(s.ItemSpacingS)
                .AlignmentV(Alignment.Vertical)
                .IsPostSizedV(true);
            for (int y = 0; y < 2; y++)
            {
                Node(craftingVert, out var craftingHor)
                    .Mut(s.HorizontalList)
                    .SizeInnerMaxRelativeV(s.Vertical)
                    .InnerSpacingV(s.ItemSpacingS);

                for (int x = 0; x < 2; x++)
                {
                    Vector2i loc = (x, y);
                    Ent val = default;

                    Node(craftingHor, out var square)
                        .Mut(s.Button)
                        .Mut(s.Slot)
                        .PlayerV((EntMut)player.Ent)
                        .GetSlotValueF(() => val)
                        .SetSlotValueF((v) => val = v);
                    {
                        Node(square)
                            .Mut(s.SlotButton)
                            .Mut(s.SlotTooltip)
                            .SlotV(square);
                    }
                }
            }

            Node(playerHor, out var arrow)
                .SizeRelativeV((0, 0))
                .SizeV((s.SlotSize, s.SlotSize))
                .AlignmentV(Alignment.Vertical)
                .IsPostSizedV(true)
                .ColorV((0, 1, 1, 1));

            Ent outputVal = default;

            Node(playerHor, out var output)
                .Mut(s.Button)
                .Mut(s.Slot)
                .PlayerV((EntMut)player.Ent)
                .AlignmentV(Alignment.Vertical)
                .IsPostSizedV(true)
                .GetSlotValueF(() => outputVal)
                .SetSlotValueF((v) => outputVal = v);
            {
                Node(output)
                    .Mut(s.SlotButton)
                    .Mut(s.SlotTooltip)
                    .SlotV(output);
            }
        }

        Node(vert, out var inventoryVert)
            .Mut(s.VerticalList)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .InnerSpacingV(s.ItemSpacingS);
        for (int y = 0; y < InventorySlots.Rows; y++)
        {
            Node(inventoryVert, out var inventoryHor)
                .Mut(s.HorizontalList)
                .SizeInnerMaxRelativeV(s.Vertical)
                .InnerSpacingV(s.ItemSpacingS);

            for (int x = 0; x < HotBarSlots.Count; x++)
            {
                Vector2i loc = (x, y);

                Node(inventoryHor, out var square)
                    .Mut(s.Button)
                    .Mut(s.Slot)
                    .PlayerV((EntMut)player.Ent)
                    .GetSlotValueF(() => player.Ent.InventorySlots()[loc.Y * HotBarSlots.Count + loc.X])
                    .SetSlotValueF((v) => player.Ent.InventorySlots()[loc.Y * HotBarSlots.Count + loc.X] = v);
                {
                    Node(square)
                        .Mut(s.SlotButton)
                        .Mut(s.SlotTooltip)
                        .SlotV(square);
                }
            }
        }

        Node(vert, out var hotbar)
            .Mut(s.HorizontalList)
            .SizeInnerMaxRelativeV(s.Vertical)
            .InnerSpacingV(s.ItemSpacingS);
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
