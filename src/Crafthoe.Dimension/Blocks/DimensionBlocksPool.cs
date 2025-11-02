namespace Crafthoe.Dimension;

[Dimension]
public class DimensionBlocksPool
{
    private readonly ConcurrentBag<Memory<Ent>> pool = [];

    public Memory<Ent> Take()
    {
        if (pool.TryTake(out var mem))
        {
            mem.Span.Clear();
            return mem;
        }

        return new Ent[ChunkVolume];
    }

    public void Add(Memory<Ent> mem) => pool.Add(mem);
}
