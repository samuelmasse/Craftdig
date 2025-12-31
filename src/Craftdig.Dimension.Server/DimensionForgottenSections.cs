namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionForgottenSections
{
    private readonly ConcurrentQueue<(EntMut, Vector3i)> queue = [];

    public void Tick()
    {
        int count = queue.Count;

        while (count > 0 && queue.TryDequeue(out var entry))
        {
            var (ent, sloc) = entry;

            ref var sections = ref ent.SocketForgottenSections();
            sections ??= [];

            ref var queue = ref ent.SocketForgottenSectionQueue();
            queue ??= [];

            if (sections.Add(sloc))
                queue.Enqueue(sloc);

            count--;
        }
    }

    public void Add(EntMut ent, Vector3i cloc) => queue.Enqueue((ent, cloc));
}
