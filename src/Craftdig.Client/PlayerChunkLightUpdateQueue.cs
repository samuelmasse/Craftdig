namespace Craftdig;

[Player]
public class PlayerChunkLightUpdateQueue
{
    private readonly ConcurrentQueue<PlayerChunkLightUpdate> queue = [];

    public int Count => queue.Count;

    public void Enqueue(PlayerChunkLightUpdate update) => queue.Enqueue(update);
    public bool TryDequeue(out PlayerChunkLightUpdate update) => queue.TryDequeue(out update);
}
