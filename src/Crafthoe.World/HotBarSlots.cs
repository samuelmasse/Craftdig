namespace Crafthoe.World;

public struct HotBarSlots
{
    public const int Count = 9;

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
