namespace Craftdig.World;

public static class HotBarSlots
{
    public const int Count = 9;

    public static ItemSlot GetHotBarSlot<T>(this T ent, int index) where T : IEntMut
    {
        var hotBarSlotCounts = ent.HotBarSlotCounts;
        var hotBarSlotEnts = ent.HotBarSlotEnts;
        if (hotBarSlotCounts == null || hotBarSlotEnts == null)
            return default;

        return new(hotBarSlotEnts[index], hotBarSlotCounts[index]);
    }

    public static void SetHotBarSlot<T>(this T ent, int index, ItemSlot val) where T : IEntMut
    {
        var hotBarSlotCounts = ent.HotBarSlotCounts ??= new int[Count];
        var hotBarSlotEnts = ent.HotBarSlotEnts ??= new Ent[Count];

        hotBarSlotEnts[index] = val.Item;
        hotBarSlotCounts[index] = val.Count;
    }
}
