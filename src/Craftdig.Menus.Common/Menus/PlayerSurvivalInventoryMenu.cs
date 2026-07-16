namespace Craftdig.Menus.Common;

[Player]
public class PlayerSurvivalInventoryMenu(
    AppStyle s,
    PlayerEnt player,
    PlayerInventoryActions inventoryActions)
{
    public void Create(EntMut root)
    {
        Node(root, out var vert)
            .Mutate(s.VerticalList)
            .SizeInnerMaxRelativeV(s.Horizontal)
            .PaddingV((s.ItemSpacing, s.ItemSpacing, s.ItemSpacing, s.ItemSpacing))
            .InnerSpacingV(s.ItemSpacing)
            .ColorV(s.BoardColor)
            .IsSelectableV(true)
            .AlignmentV(Alignment.Center);

        Node(vert, out var playerHor)
            .Mutate(s.HorizontalList)
            .InnerSizingV(InnerSizing.HorizontalWeight)
            .InnerSpacingV(s.ItemSpacingS)
            .SizeInnerMaxRelativeV(s.Vertical)
            .SizeRelativeV(s.Horizontal)
            .SizeInnerSumRelativeV(default)
            .IsPostSizedV(true);
        {
            Node(playerHor, out var armorVert)
                .Mutate(s.VerticalList)
                .SizeInnerMaxRelativeV(s.Horizontal)
                .SizeWeightTypeV(SizeWeightType.Self);
            for (int i = 0; i < Slots.ArmorCount; i++)
            {
                int x = i;

                Node(armorVert, out var square)
                    .Mutate(s.Button)
                    .Mutate(s.Slot)
                    .PlayerV(player)
                    .GetSlotValueF(() => player.ArmorSlots[x])
                    .SetSlotValueF((v) => player.ArmorSlots[x] = v)
                    .InventoryClickF(click => inventoryActions.Submit(
                        InventoryOperation.ClickSlot(InventoryContainer.Armor, x, click)));
                {
                    Node(square)
                        .Mutate(s.SlotButton)
                        .Mutate(s.SlotTooltip)
                        .SlotV(square);
                }
            }

            Node(playerHor, out var playerDisplay)
                .ColorV((0, 0, 0, 1))
                .SizeV((s.SlotSize * 3, 0))
                .SizeRelativeV(s.Vertical)
                .IsPostSizedV(true)
                .SizeWeightTypeV(SizeWeightType.Self);

            Node(playerHor, out var craftingArea)
                .SizeRelativeV(s.Vertical)
                .IsPostSizedV(true);
            {
                Node(craftingArea, out var craftingHor)
                    .Mutate(s.HorizontalList)
                    .SizeRelativeV(s.Vertical)
                    .InnerSpacingV(s.ItemSpacingS)
                    .AlignmentV(Alignment.Horizontal);

                Node(craftingHor, out var craftingVert)
                    .Mutate(s.VerticalList)
                    .SizeInnerMaxRelativeV(s.Horizontal)
                    .AlignmentV(Alignment.Vertical);
                {
                    Node(craftingVert, out var title)
                        .Mutate(s.Label)
                        .TextV("Crafting")
                        .IsFloatingV(true)
                        .AlignmentV(Alignment.Horizontal)
                        .OffsetTextRelativeV(-s.Vertical)
                        .OffsetV((0, -s.ItemSpacingXS));

                    for (int y = 0; y < 2; y++)
                    {
                        Node(craftingVert, out var craftingGridHor)
                            .Mutate(s.HorizontalList)
                            .SizeInnerMaxRelativeV(s.Vertical);

                        for (int x = 0; x < 2; x++)
                        {
                            Vec2i loc = (x, y);
                            ItemSlot val = default;

                            Node(craftingGridHor, out var square)
                                .Mutate(s.Button)
                                .Mutate(s.Slot)
                                .PlayerV(player)
                                .GetSlotValueF(() => val)
                                .SetSlotValueF((v) => val = v);
                            {
                                Node(square)
                                    .Mutate(s.SlotButton)
                                    .Mutate(s.SlotTooltip)
                                    .SlotV(square);
                            }
                        }
                    }
                }

                Node(craftingHor, out var arrow)
                    .SizeRelativeV((0, 0))
                    .SizeV((s.SlotSize, s.SlotSize))
                    .AlignmentV(Alignment.Vertical)
                    .TextureV(s.ArrowTexture);

                ItemSlot outputVal = default;

                Node(craftingHor, out var output)
                    .Mutate(s.Button)
                    .Mutate(s.Slot)
                    .PlayerV(player)
                    .AlignmentV(Alignment.Vertical)
                    .GetSlotValueF(() => outputVal)
                    .SetSlotValueF((v) => outputVal = v);
                {
                    Node(output)
                        .Mutate(s.SlotButton)
                        .Mutate(s.SlotTooltip)
                        .SlotV(output);
                }
            }
        }

        Node(vert, out var inventoryVert)
            .Mutate(s.VerticalList)
            .SizeInnerMaxRelativeV(s.Horizontal);
        for (int y = 0; y < Slots.InventoryRows; y++)
        {
            Node(inventoryVert, out var inventoryHor)
                .Mutate(s.HorizontalList)
                .SizeInnerMaxRelativeV(s.Vertical);

            for (int x = 0; x < Slots.HotBarCount; x++)
            {
                Vec2i loc = (x, y);

                Node(inventoryHor, out var square)
                    .Mutate(s.Button)
                    .Mutate(s.Slot)
                    .PlayerV(player)
                    .GetSlotValueF(() => player.InventorySlots[loc.Y * Slots.HotBarCount + loc.X])
                    .SetSlotValueF((v) => player.InventorySlots[loc.Y * Slots.HotBarCount + loc.X] = v)
                    .InventoryClickF(click => inventoryActions.Submit(
                        InventoryOperation.ClickSlot(
                            InventoryContainer.Inventory,
                            loc.Y * Slots.HotBarCount + loc.X,
                            click)));
                {
                    Node(square)
                        .Mutate(s.SlotButton)
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
                .InventoryClickF(click => inventoryActions.Submit(
                    InventoryOperation.ClickSlot(InventoryContainer.HotBar, i, click)))
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
