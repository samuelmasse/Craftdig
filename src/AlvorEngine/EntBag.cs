namespace AlvorEngine;

public class EntBag<E, N> where E : IEntSet<E>
{
    private E[] ents = [default!, default!];
    private int count = 1;

    public ReadOnlySpan<E> Ents => new(ents, 1, count - 1);

    public void Add(E ent)
    {
        ent.Set<int, N>(count);
        if (count >= ents.Length)
            Array.Resize(ref ents, ents.Length * 2);
        ents[count++] = ent;
    }

    public void Remove(E ent)
    {
        if (!Contains(ent))
            return;

        int index = ent.Get<int, N>();
        ref var last = ref ents[count - 1];
        ents[index] = last;
        last.Set<int, N>(index);
        last = default;
        count--;
    }

    public bool Contains(E chunk) => chunk.Has<int, N>();
}
