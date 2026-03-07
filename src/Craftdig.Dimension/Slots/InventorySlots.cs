namespace Craftdig.Dimension;

public static class InventorySlots
{
    public const int Rows = 3;
    public const int Count = HotBarSlots.Count * Rows;

    public static ItemSlot GetInventorySlot<T>(this T ent, int index) where T : IEntMut
    {
        var inventorySlotCounts = ent.InventorySlotCounts;
        var inventorySlotEnts = ent.InventorySlotEnts;
        if (inventorySlotCounts == null || inventorySlotEnts == null)
            return default;

        return new(inventorySlotEnts[index], inventorySlotCounts[index]);
    }

    public static void SetInventorySlot<T>(this T ent, int index, ItemSlot val) where T : IEntMut
    {
        var inventorySlotCounts = ent.InventorySlotCounts ??= new int[Count];
        var inventorySlotEnts = ent.InventorySlotEnts ??= new Ent[Count];

        inventorySlotEnts[index] = val.Item;
        inventorySlotCounts[index] = val.Count;
    }
}
