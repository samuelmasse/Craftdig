namespace Craftdig;

[Dimension]
public class DimensionInventoryOperations(WorldModuleIndices moduleIndices)
{
    public InventoryApplyResult Apply(IEntMut ent, in InventoryOperation operation)
    {
        if (operation.Kind == InventoryActionKind.SelectHotBar)
            return SelectHotBar(ent, operation.Slot);

        if (operation.Kind == InventoryActionKind.CreativeClick)
            return CreativeClick(ent, operation.ModuleIndex, operation.Click);

        if (operation.Kind != InventoryActionKind.Click || !TryGetSlot(ent, operation.Container, operation.Slot, out var slot))
            return InventoryApplyResult.Rejected;

        var cursor = ent.Offhand;
        if (!IsValid(slot) || !IsValid(cursor))
            return InventoryApplyResult.Rejected;

        var previousSlot = slot;
        var previousCursor = cursor;
        if (operation.Click == InventoryClick.Primary)
            PrimaryClick(ref slot, ref cursor);
        else if (operation.Click == InventoryClick.Secondary)
            SecondaryClick(ref slot, ref cursor);
        else return InventoryApplyResult.Rejected;

        if (!IsValid(slot) || !IsValid(cursor))
            return InventoryApplyResult.Rejected;
        if (slot == previousSlot && cursor == previousCursor)
            return InventoryApplyResult.Accepted;

        SetSlot(ent, operation.Container, operation.Slot, slot);
        ent.Offhand = cursor;
        return InventoryApplyResult.Changed;
    }

    private InventoryApplyResult CreativeClick(
        IEntMut ent,
        int moduleIndex,
        InventoryClick click)
    {
        if (!moduleIndices.Contains(moduleIndex))
            return InventoryApplyResult.Rejected;

        var item = moduleIndices[moduleIndex];
        if (!IsItem(item))
            return InventoryApplyResult.Rejected;

        var cursor = ent.Offhand;
        if (!IsValid(cursor))
            return InventoryApplyResult.Rejected;

        var previous = cursor;
        if (click == InventoryClick.Primary)
        {
            if (cursor.Item == item && cursor.Count < item.MaxStack)
                cursor = new(item, cursor.Count + 1);
            else if (cursor.Item != item)
                cursor = new(item, 1);
        }
        else if (click == InventoryClick.Secondary)
        {
            if (cursor == default)
                cursor = new(item, 1);
            else if (cursor.Count == 1)
                cursor = default;
            else cursor = new(cursor.Item, cursor.Count - 1);
        }
        else return InventoryApplyResult.Rejected;

        if (cursor == previous)
            return InventoryApplyResult.Accepted;

        ent.Offhand = cursor;
        return InventoryApplyResult.Changed;
    }

    private InventoryApplyResult SelectHotBar(IEntMut ent, int slot)
    {
        if (slot >= Slots.HotBarCount)
            return InventoryApplyResult.Rejected;
        if (ent.HotBarIndex == slot)
            return InventoryApplyResult.Accepted;

        ent.HotBarIndex = slot;
        return InventoryApplyResult.Changed;
    }

    private bool TryGetSlot(IEntMut ent, InventoryContainer container, int index, out ItemSlot slot)
    {
        int count = container switch
        {
            InventoryContainer.Armor => Slots.ArmorCount,
            InventoryContainer.Inventory => Slots.InventoryCount,
            InventoryContainer.HotBar => Slots.HotBarCount,
            _ => 0,
        };
        if (index >= count)
        {
            slot = default;
            return false;
        }

        slot = container switch
        {
            InventoryContainer.Armor => ent.ArmorSlots[index],
            InventoryContainer.Inventory => ent.InventorySlots[index],
            InventoryContainer.HotBar => ent.HotBarSlots[index],
            _ => default,
        };
        return true;
    }

    private void SetSlot(IEntMut ent, InventoryContainer container, int index, in ItemSlot slot)
    {
        if (container == InventoryContainer.Armor)
            ent.ArmorSlots[index] = slot;
        else if (container == InventoryContainer.Inventory)
            ent.InventorySlots[index] = slot;
        else ent.HotBarSlots[index] = slot;
    }

    private void PrimaryClick(ref ItemSlot slot, ref ItemSlot cursor)
    {
        if (cursor == default)
        {
            cursor = slot;
            slot = default;
        }
        else if (slot.Item == cursor.Item)
        {
            int give = Math.Min(cursor.Count, slot.Item.MaxStack - slot.Count);
            if (give == 0)
                return;

            slot = new(slot.Item, slot.Count + give);
            cursor = cursor.Count == give ? default : new(cursor.Item, cursor.Count - give);
        }
        else (slot, cursor) = (cursor, slot);
    }

    private void SecondaryClick(ref ItemSlot slot, ref ItemSlot cursor)
    {
        if (cursor == default)
        {
            int give = (slot.Count + 1) / 2;
            if (give == 0)
                return;

            cursor = new(slot.Item, give);
            slot = slot.Count == give ? default : new(slot.Item, slot.Count - give);
        }
        else if (slot.Item == default || slot.Item == cursor.Item)
        {
            if (slot.Count == cursor.Item.MaxStack)
                return;

            slot = new(cursor.Item, slot.Count + 1);
            cursor = cursor.Count == 1 ? default : new(cursor.Item, cursor.Count - 1);
        }
        else (slot, cursor) = (cursor, slot);
    }

    private bool IsValid(ItemSlot slot) => slot == default ||
        IsItem(slot.Item) && slot.Count > 0 && slot.Count <= slot.Item.MaxStack;

    private bool IsItem(Ent item) => item != default && item.IsBlock && item.IsBuildable && item.MaxStack > 0;

}
