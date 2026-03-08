namespace Craftdig.Dimension;

public static class Slots
{
    public const int ArmorCount = 4;
    public const int HotBarCount = 9;

    public const int InventoryRows = 3;
    public const int IntentoyCount = HotBarCount * InventoryRows;

    extension<T>(T ent) where T : IEntMut
    {
        public ItemSlot Offhand
        {
            get => new(ent.OffhandEnt, ent.OffhandCount);
            set
            {
                ent.Mutate().OffhandEnt();
                ent.OffhandEnt = value.Item;
                ent.OffhandCount = value.Count;
            }
        }

        public SlotArray ArmorSlots => From<
            DimensionComponents.ArmorSlotCounts,
            DimensionComponents.ArmorSlotEnts,
            T>(ent, ArmorCount);

        public SlotArray HotBarSlots => From<
            DimensionComponents.HotBarSlotCounts,
            DimensionComponents.HotBarSlotEnts,
            T>(ent, HotBarCount);

        public SlotArray InventorySlots => From<
            DimensionComponents.InventorySlotCounts,
            DimensionComponents.InventorySlotEnts,
            T>(ent, IntentoyCount);
    }

    public static SlotArray<C, E> From<C, E, T>(T ent, int length) where T : IEntMut
    {
        var array = ent.Get<SlotArray<C, E>, SlotArray<C, E>>();
        if (array == null)
        {
            array = new SlotArray<C, E>(ent, length);
            ent.Set<SlotArray<C, E>, SlotArray<C, E>>(array);
        }

        return array;
    }

    extension<T>(EntMutator<T> mut) where T : IEntMut
    {
        public EntMutator<T> Offhand(in ItemSlot value)
        {
            mut.Ent.Offhand = value;
            return mut;
        }

        public ItemSlot Offhand() => mut.Ent.Offhand;
    }
}
