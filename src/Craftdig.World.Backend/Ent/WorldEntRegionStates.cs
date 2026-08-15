namespace Craftdig;

[World]
public class WorldEntRegionStates(
    WorldPaths paths,
    WorldEntRegionBuckets buckets,
    WorldEntRegionReader regionReader)
{
    private readonly Dictionary<Vec2i, EntRegionState> dict = [];

    public EntRegionState this[Vec2i rloc] => EnsureLoaded(rloc);

    public EntRegionState EnsureLoaded(Vec2i rloc)
    {
        if (!dict.TryGetValue(rloc, out var state))
        {
            state = New(rloc);
            dict.Add(rloc, state);
        }

        return state;
    }

    private EntRegionState New(Vec2i rloc)
    {
        var state = new EntRegionState(Dir(rloc), rloc, buckets.Count);
        regionReader.ReadEntsFromRegion(state);
        return state;
    }

    protected virtual string Dir(Vec2i rloc) => Path.Join(paths.Ents, "World");
}
