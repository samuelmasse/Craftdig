namespace Crafthoe.World;

public struct ArmorSlots
{
    public const int Count = 4;

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
