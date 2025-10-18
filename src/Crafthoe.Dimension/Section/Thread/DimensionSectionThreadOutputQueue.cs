namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionThreadOutputQueue
{
    private readonly ConcurrentQueue<SectionThreadOutput> q = [];

    public void Enqeue(SectionThreadOutput output) => q.Enqueue(output);
    public bool TryDequeue([MaybeNullWhen(false)] out SectionThreadOutput output) => q.TryDequeue(out output);
}
