namespace Craftdig;

[Dimension]
public class DimensionChunkThreadOutputBag
{
    private readonly ConcurrentQueue<ChunkThreadOutput> queue = [];

    public int Count => queue.Count;

    public void Add(ChunkThreadOutput output) => queue.Enqueue(output);
    public bool TryTake(out ChunkThreadOutput output) => queue.TryDequeue(out output);
}
