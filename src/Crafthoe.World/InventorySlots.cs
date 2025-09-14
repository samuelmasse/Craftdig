namespace Crafthoe.World;

public struct InventorySlots
{
    public const int Rows = 3;
    public const int Count = HotBarSlots.Count * Rows;

    private Ent[]? slots;

    public ref Ent this[int index]
    {
        get
        {
            slots ??= new Ent[Count];
            return ref slots[index];
        }
    }
}
