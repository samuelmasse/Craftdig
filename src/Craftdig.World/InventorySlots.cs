namespace Craftdig.World;

public struct InventorySlots
{
    public const int Rows = 3;
    public const int Count = HotBarSlots.Count * Rows;

    private ItemSlot[]? slots;

    public ref ItemSlot this[int index]
    {
        get
        {
            slots ??= new ItemSlot[Count];
            return ref slots[index];
        }
    }
}
