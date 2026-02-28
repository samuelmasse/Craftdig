namespace Craftdig.World;

public static class InventorySlots
{
    public const int Rows = 3;
    public const int Count = HotBarSlots.Count * Rows;

    public static ItemSlot GetInventorySlot<T>(this T ent, int index) where T : IEntMut
    {
        var inventorySlotCounts = ent.GetInventorySlotCounts();
        var inventorySlotEntities = ent.GetInventorySlotEntities();
        if (inventorySlotCounts == null || inventorySlotEntities == null)
            return default;

        return new(inventorySlotEntities[index], inventorySlotCounts[index]);
    }

    public static void SetInventorySlot<T>(this T ent, int index, ItemSlot val) where T : IEntMut
    {
        ref var inventorySlotCounts = ref ent.InventorySlotCounts();
        ref var inventorySlotEntities = ref ent.InventorySlotEntities();

        inventorySlotCounts ??= new int[Count];
        inventorySlotEntities ??= new Ent[Count];

        inventorySlotEntities[index] = val.Item;
        inventorySlotCounts[index] = val.Count;
    }
}
