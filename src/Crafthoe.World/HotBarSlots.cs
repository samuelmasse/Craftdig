namespace Crafthoe.World;

public struct HotBarSlots
{
    public const int Count = 9;

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
