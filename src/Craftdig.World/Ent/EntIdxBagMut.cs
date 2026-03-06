namespace Craftdig.World;

public class EntIdxBagMut<N>
{
    private EntMutIdx[] ents = [default!, default!];
    private int count = 1;

    public ReadOnlySpan<EntMutIdx> Ents => new(ents, 1, count - 1);

    public void Add(EntMutIdx ent)
    {
        ent.Set<int, EntIdxBagIndex<N>>(count);
        if (count >= ents.Length)
            Array.Resize(ref ents, ents.Length * 2);
        ents[count++] = ent;
    }

    public void Remove(EntMutIdx ent)
    {
        if (!Contains(ent))
            return;

        int index = ent.Get<int, EntIdxBagIndex<N>>();
        ref var last = ref ents[count - 1];
        ents[index] = last;
        last.Set<int, EntIdxBagIndex<N>>(index);
        last = default;
        count--;
    }

    public bool Contains(EntMutIdx ent) => ent.Has<int, EntIdxBagIndex<N>>();
}
