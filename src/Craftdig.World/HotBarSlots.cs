namespace Craftdig.World;

public static class HotBarSlots
{
    public const int Count = 9;

    public static ItemSlot GetHotBarSlot<T>(this T ent, int index) where T : IEntMut
    {
        var hotBarSlotCounts = ent.HotBarSlotCounts;
        var hotBarSlotEntities = ent.HotBarSlotEntities;
        if (hotBarSlotCounts == null || hotBarSlotEntities == null)
            return default;

        return new(hotBarSlotEntities[index], hotBarSlotCounts[index]);
    }

    public static void SetHotBarSlot<T>(this T ent, int index, ItemSlot val) where T : IEntMut
    {
        var hotBarSlotCounts = ent.HotBarSlotCounts ??= new int[Count];
        var hotBarSlotEntities = ent.HotBarSlotEntities ??= new Ent[Count];

        hotBarSlotEntities[index] = val.Item;
        hotBarSlotCounts[index] = val.Count;
    }
}
