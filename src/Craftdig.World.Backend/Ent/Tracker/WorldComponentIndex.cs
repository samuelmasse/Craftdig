namespace Craftdig.World.Backend;

[World]
public class WorldComponentIndex<T, N>(WorldComponentIndices indices)
{
    private readonly int index = indices[new EntComponent(typeof(T), typeof(N))];

    public int Index => index;
}
