namespace Craftdig.Player.Frontend;

public sealed class PlayerInventorySnapshot
{
    private readonly ItemSlot[] armor = new ItemSlot[Slots.ArmorCount];
    private readonly ItemSlot[] inventory = new ItemSlot[Slots.InventoryCount];
    private readonly ItemSlot[] hotBar = new ItemSlot[Slots.HotBarCount];
    private ItemSlot cursor;
    private int hotBarIndex;

    public long Revision { get; private set; }

    public void Capture(PlayerEnt player)
    {
        Capture(
            player.Get<Ent[]?, DimensionComponents.ArmorSlotEnts>(),
            player.Get<int[]?, DimensionComponents.ArmorSlotCounts>(),
            armor);
        Capture(
            player.Get<Ent[]?, DimensionComponents.InventorySlotEnts>(),
            player.Get<int[]?, DimensionComponents.InventorySlotCounts>(),
            inventory);
        Capture(
            player.Get<Ent[]?, DimensionComponents.HotBarSlotEnts>(),
            player.Get<int[]?, DimensionComponents.HotBarSlotCounts>(),
            hotBar);
        cursor = player.Offhand;
        hotBarIndex = player.HotBarIndex;
        Revision = player.InventoryRevision;
    }

    public void Restore(PlayerEnt player)
    {
        Restore<DimensionComponents.ArmorSlotEnts, DimensionComponents.ArmorSlotCounts>(player, armor);
        Restore<DimensionComponents.InventorySlotEnts, DimensionComponents.InventorySlotCounts>(player, inventory);
        Restore<DimensionComponents.HotBarSlotEnts, DimensionComponents.HotBarSlotCounts>(player, hotBar);
        player.Offhand = cursor;
        player.HotBarIndex = hotBarIndex;
        player.InventoryRevision = Revision;
    }

    private void Capture(Ent[]? items, int[]? counts, Span<ItemSlot> destination)
    {
        destination.Clear();
        if (items == null || counts == null)
            return;

        int count = Math.Min(destination.Length, Math.Min(items.Length, counts.Length));
        for (int i = 0; i < count; i++)
            destination[i] = new(items[i], counts[i]);
    }

    private void Restore<E, C>(PlayerEnt player, ReadOnlySpan<ItemSlot> source)
    {
        var items = player.Get<Ent[]?, E>();
        var counts = player.Get<int[]?, C>();
        if (items == null || items.Length != source.Length)
            items = new Ent[source.Length];
        if (counts == null || counts.Length != source.Length)
            counts = new int[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            items[i] = source[i].Item;
            counts[i] = source[i].Count;
        }

        player.Set<Ent[]?, E>(items);
        player.Set<int[]?, C>(counts);
    }
}
