namespace Craftdig.World.Backend;

[World]
public class WorldEntRegionStates(
    WorldPaths paths,
    WorldEntRegionBuckets buckets,
    WorldEntRegionReader regionReader)
{
    private readonly Dictionary<Vector2i, EntRegionState> dict = [];

    public EntRegionState this[Vector2i rloc] => EnsureLoaded(rloc);

    public EntRegionState EnsureLoaded(Vector2i rloc)
    {
        if (!dict.TryGetValue(rloc, out var state))
        {
            state = New(rloc);
            dict.Add(rloc, state);
        }

        return state;
    }

    private EntRegionState New(Vector2i rloc)
    {
        var state = new EntRegionState(Dir(rloc), rloc, buckets.Count);
        regionReader.ReadEntsFromRegion(state);
        return state;
    }

    protected virtual string Dir(Vector2i rloc) => Path.Join(paths.Ents, "World");
}
