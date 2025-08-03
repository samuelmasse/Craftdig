namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkIndex
{
    private EntMut[] chunks = [default!];
    private int count;

    public ReadOnlySpan<EntMut> Chunks => new(chunks, 0, count);

    public void Add(EntMut chunk)
    {
        chunk.Set<int, DimensionChunkIndex>(count);
        if (count >= chunks.Length)
            Array.Resize(ref chunks, chunks.Length * 2);
        chunks[count++] = chunk;
    }

    public void Remove(Ent chunk)
    {
        if (!Contains(chunk))
            return;

        int index = chunk.Get<int, DimensionChunkIndex>();
        ref var last = ref chunks[count - 1];
        chunks[index] = last;
        last.Set<int, DimensionChunkIndex>(index);
        last = default;
        count--;
    }

    public bool Contains(Ent chunk) => chunk.Has<int, DimensionChunkIndex>();
}
