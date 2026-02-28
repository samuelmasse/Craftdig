namespace Craftdig.World;

public static class HotBarSlots
{
    public const int Count = 9;

    public static ItemSlot GetHotBarSlot<T>(this T ent, int index) where T : IEntMut
    {
        var hotBarSlotCounts = ent.GetHotBarSlotCounts();
        var hotBarSlotEntities = ent.GetHotBarSlotEntities();
        if (hotBarSlotCounts == null || hotBarSlotEntities == null)
            return default;

        return new(hotBarSlotEntities[index], hotBarSlotCounts[index]);
    }

    public static void SetHotBarSlot<T>(this T ent, int index, ItemSlot val) where T : IEntMut
    {
        ref var hotBarSlotCounts = ref ent.HotBarSlotCounts();
        ref var hotBarSlotEntities = ref ent.HotBarSlotEntities();

        hotBarSlotCounts ??= new int[Count];
        hotBarSlotEntities ??= new Ent[Count];

        hotBarSlotEntities[index] = val.Item;
        hotBarSlotCounts[index] = val.Count;
    }
}
