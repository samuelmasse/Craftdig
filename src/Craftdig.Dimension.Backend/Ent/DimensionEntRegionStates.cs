namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntRegionStates(
    DimensionPaths paths,
    DimensionEntRegionBuckets buckets,
    DimensionEntRegionReader regionReader)
{
    private readonly Dictionary<Vector2i, EntRegionState> dict = [];

    public EntRegionState this[Vector2i rloc]
    {
        get
        {
            if (!dict.TryGetValue(rloc, out var state))
            {
                state = New(rloc);
                dict.Add(rloc, state);
            }

            return state;
        }
    }

    private EntRegionState New(Vector2i rloc)
    {
        var state = new EntRegionState(paths.Ents, rloc, buckets.Count);
        regionReader.ReadEntsFromRegion(state);
        return state;
    }
}
