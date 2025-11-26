namespace Crafthoe.Client;

[Player]
public class PlayerSectionUpdateQueue
{
    private readonly ConcurrentQueue<(Vector3i, Ent[])> queue = [];

    public int Count => queue.Count;

    public void Enqueue((Vector3i, Ent[]) item) => queue.Enqueue(item);
    public bool TryDequeue(out (Vector3i, Ent[]) item) => queue.TryDequeue(out item);
}
