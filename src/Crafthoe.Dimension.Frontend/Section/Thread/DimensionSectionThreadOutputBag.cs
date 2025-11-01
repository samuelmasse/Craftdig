namespace Crafthoe.Dimension.Frontend;

[Dimension]
public class DimensionSectionThreadOutputBag
{
    private readonly ConcurrentBag<SectionThreadOutput> bag = [];

    public int Count => bag.Count;

    public void Add(SectionThreadOutput output) => bag.Add(output);
    public bool TryTake([MaybeNullWhen(false)] out SectionThreadOutput output) => bag.TryTake(out output);
}
