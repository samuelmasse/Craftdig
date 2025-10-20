namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkThreadOutputBag
{
    private readonly ConcurrentBag<ChunkThreadInput> bag = [];

    public int Count => bag.Count;

    public void Add(ChunkThreadInput output) => bag.Add(output);
    public bool TryTake([MaybeNullWhen(false)] out ChunkThreadInput output) => bag.TryTake(out output);
}
