namespace Craftdig.World;

public static class ArmorSlots
{
    public const int Count = 4;

    public static ItemSlot GetArmorSlot<T>(this T ent, int index) where T : IEntMut
    {
        var armorSlotCounts = ent.ArmorSlotCounts;
        var armorSlotEnts = ent.ArmorSlotEnts;
        if (armorSlotCounts == null || armorSlotEnts == null)
            return default;

        return new(armorSlotEnts[index], armorSlotCounts[index]);
    }

    public static void SetArmorSlot<T>(this T ent, int index, ItemSlot val) where T : IEntMut
    {
        var armorSlotCounts = ent.ArmorSlotCounts ??= new int[Count];
        var armorSlotEnts = ent.ArmorSlotEnts ??= new Ent[Count];

        armorSlotEnts[index] = val.Item;
        armorSlotCounts[index] = val.Count;
    }
}
