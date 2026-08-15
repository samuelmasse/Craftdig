namespace Craftdig;

[Dimension]
public class DimensionForgottenSections
{
    private readonly ConcurrentQueue<(NetSocket, Vec3i)> queue = [];

    public void Tick()
    {
        int count = queue.Count;

        while (count > 0 && queue.TryDequeue(out var entry))
        {
            var (ent, sloc) = entry;

            var sections = ent.SocketForgottenSections ??= [];
            var queue = ent.SocketForgottenSectionQueue ??= [];

            if (sections.Add(sloc))
                queue.Enqueue(sloc);

            count--;
        }
    }

    public void Add(NetSocket ent, Vec3i cloc) => queue.Enqueue((ent, cloc));
}
