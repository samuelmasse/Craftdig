namespace Crafthoe.Client;

[Player]
public class PlayerChunkUpdateQueue
{
    private readonly ConcurrentQueue<(Vector2i, Memory<Ent>)> queue = [];

    public int Count => queue.Count;

    public void Enqueue((Vector2i, Memory<Ent>) item) => queue.Enqueue(item);
    public bool TryDequeue(out (Vector2i, Memory<Ent>) item) => queue.TryDequeue(out item);
}
