namespace Craftdig;

public abstract class SlotArray
{
    public abstract ItemSlot this[int index] { get; set; }
}

public class SlotArray<C, E>(IEntMut ent, int length) : SlotArray
{
    public override ItemSlot this[int index]
    {
        get
        {
            var counts = ent.Get<int[]?, C>();
            var ents = ent.Get<Ent[]?, E>();
            if (counts == null || ents == null)
                return default;

            return new(ents[index], counts[index]);
        }
        set
        {
            if (value == this[index])
                return;

            var counts = ent.Get<int[]?, C>() ?? new int[length];
            var ents = ent.Get<Ent[]?, E>() ?? new Ent[length];

            ents[index] = value.Item;
            counts[index] = value.Count;

            ent.Set<int[]?, C>(counts);
            ent.Set<Ent[]?, E>(ents);
        }
    }
}
