namespace Craftdig.World;

public static class ArmorSlots
{
    public const int Count = 4;

    public static ItemSlot GetArmorSlot<T>(this T ent, int index) where T : IEntMut
    {
        var armorSlotCounts = ent.ArmorSlotCounts;
        var armorSlotEntities = ent.ArmorSlotEntities;
        if (armorSlotCounts == null || armorSlotEntities == null)
            return default;

        return new(armorSlotEntities[index], armorSlotCounts[index]);
    }

    public static void SetArmorSlot<T>(this T ent, int index, ItemSlot val) where T : IEntMut
    {
        var armorSlotCounts = ent.ArmorSlotCounts ??= new int[Count];
        var armorSlotEntities = ent.ArmorSlotEntities ??= new Ent[Count];

        armorSlotEntities[index] = val.Item;
        armorSlotCounts[index] = val.Count;
    }
}
