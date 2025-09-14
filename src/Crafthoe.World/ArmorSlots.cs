namespace Crafthoe.World;

public struct ArmorSlots
{
    public const int Count = 4;

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
