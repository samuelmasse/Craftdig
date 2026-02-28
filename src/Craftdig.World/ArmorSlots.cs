namespace Craftdig.World;

public static class ArmorSlots
{
    public const int Count = 4;

    public static ItemSlot GetArmorSlot<T>(this T ent, int index) where T : IEntMut
    {
        var armorSlotCounts = ent.GetArmorSlotCounts();
        var armorSlotEntities = ent.GetArmorSlotEntities();
        if (armorSlotCounts == null || armorSlotEntities == null)
            return default;

        return new(armorSlotEntities[index], armorSlotCounts[index]);
    }

    public static void SetArmorSlot<T>(this T ent, int index, ItemSlot val) where T : IEntMut
    {
        ref var armorSlotCounts = ref ent.ArmorSlotCounts();
        ref var armorSlotEntities = ref ent.ArmorSlotEntities();

        armorSlotCounts ??= new int[Count];
        armorSlotEntities ??= new Ent[Count];

        armorSlotEntities[index] = val.Item;
        armorSlotCounts[index] = val.Count;
    }
}
