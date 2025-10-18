namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRegionThreadOutputBag
{
    private readonly ConcurrentBag<RegionThreadOutput> bag = [];

    public void Add(RegionThreadOutput output) => bag.Add(output);
    public bool TryTake([MaybeNullWhen(false)] out RegionThreadOutput output) => bag.TryTake(out output);
}
