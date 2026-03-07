namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntRegionWriter(DimensionEntRegionStates entRegionStates)
{
    public void Write(Ent ent, Vector2i rloc)
    {
        var state = entRegionStates[rloc];
    }

    public void Erase(Ent ent, Vector2i rloc)
    {

    }
}
