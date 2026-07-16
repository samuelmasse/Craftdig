namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionForgottenChunks(DimensionEntStreamer entStreamer)
{
    private readonly ConcurrentQueue<(NetSocket, Vec2i)> queue = [];

    public void Tick()
    {
        int count = queue.Count;

        while (count > 0 && queue.TryDequeue(out var entry))
        {
            var (ent, cloc) = entry;
            var streamed = ent.SocketStreamedChunks;
            if (streamed != null && streamed.Contains(cloc))
            {
                entStreamer.LeaveChunk(ent, cloc);
                streamed.Remove(cloc);
            }
            count--;
        }
    }

    public void Add(NetSocket ent, Vec2i cloc) => queue.Enqueue((ent, cloc));
}
