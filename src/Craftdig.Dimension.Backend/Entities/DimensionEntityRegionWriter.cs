namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntityRegionWriter(DimensionEntityRegionStates entityRegionStates)
{
    public void Write(Ent ent, Vector2i rloc)
    {
        var state = entityRegionStates[rloc];
    }

    public void Erase(Ent ent, Vector2i rloc)
    {

    }
}
