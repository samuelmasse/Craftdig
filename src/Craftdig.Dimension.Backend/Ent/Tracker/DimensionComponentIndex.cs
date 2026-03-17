namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionComponentIndex<T, N>(WorldComponentIndices indices)
{
    private readonly int index = indices[new EntComponent(typeof(T), typeof(N))];

    public int Index => index;
}
