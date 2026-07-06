namespace Craftdig.Client;

[Player]
public class PlayerChunkUpdateQueue
{
    private readonly ConcurrentQueue<(Vec2i, ChunkBlocks)> queue = [];

    public int Count => queue.Count;

    public void Enqueue((Vec2i, ChunkBlocks) item) => queue.Enqueue(item);
    public bool TryDequeue(out (Vec2i, ChunkBlocks) item) => queue.TryDequeue(out item);
}
