namespace Craftdig.World;

public static class Offhand
{
    public static ItemSlot GetOffhand<T>(this T ent) where T : IEntMut
    {
        return new ItemSlot(ent.OffhandEnt, ent.OffhandCount);
    }

    public static void SetOffhand<T>(this T ent, ItemSlot val) where T : IEntMut
    {
        ent.OffhandEnt = val.Item;
        ent.OffhandCount = val.Count;
    }
}
