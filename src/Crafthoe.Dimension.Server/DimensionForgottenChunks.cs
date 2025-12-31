namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionForgottenChunks
{
    private readonly ConcurrentQueue<(Ent, Vector2i)> queue = [];

    public void Tick()
    {
        int count = queue.Count;

        while (count > 0 && queue.TryDequeue(out var entry))
        {
            var (ent, cloc) = entry;
            ent.SocketStreamedChunks()?.Remove(cloc);
            count--;
        }
    }

    public void Add(Ent ent, Vector2i cloc) => queue.Enqueue((ent, cloc));
}
