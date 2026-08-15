namespace Craftdig;

[Dimension]
public class DimensionSectionThreadOutputBag
{
    private readonly ConcurrentQueue<SectionThreadOutput> drawable = [];
    private readonly ConcurrentQueue<SectionThreadOutput> empty = [];

    public int Count => drawable.Count + empty.Count;

    public void Add(SectionThreadOutput output)
    {
        if (output.Buffer.Count > 0)
            drawable.Enqueue(output);
        else empty.Enqueue(output);
    }

    public bool TryTake([MaybeNullWhen(false)] out SectionThreadOutput output) =>
        drawable.TryDequeue(out output) || empty.TryDequeue(out output);
}
