namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntityRegionStates(DimensionPaths paths, DimensionEntityRegionBuckets buckets)
{
    private readonly Dictionary<Vector2i, EntityRegionState> dict = [];

    public EntityRegionState this[Vector2i rloc]
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

    private EntityRegionState New(Vector2i rloc)
    {
        var state = new EntityRegionState(paths.Entities, rloc, buckets.Count);
        return state;
    }
}
