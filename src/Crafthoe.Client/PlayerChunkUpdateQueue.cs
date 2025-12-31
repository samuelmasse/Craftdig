namespace Craftdig.Client;

[Player]
public class PlayerChunkUpdateQueue
{
    private readonly ConcurrentQueue<(Vector2i, ChunkBlocks)> queue = [];

    public int Count => queue.Count;

    public void Enqueue((Vector2i, ChunkBlocks) item) => queue.Enqueue(item);
    public bool TryDequeue(out (Vector2i, ChunkBlocks) item) => queue.TryDequeue(out item);
}
