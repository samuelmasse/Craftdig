namespace Craftdig.Client;

[Player]
public class PlayerSectionUpdateQueue
{
    private readonly ConcurrentQueue<(Vec3i, Ent[])> queue = [];

    public int Count => queue.Count;

    public void Enqueue((Vec3i, Ent[]) item) => queue.Enqueue(item);
    public bool TryDequeue(out (Vec3i, Ent[]) item) => queue.TryDequeue(out item);
}
