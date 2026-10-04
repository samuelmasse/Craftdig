namespace Craftdig;

[World]
public class WorldComponentIndex<T, N>(WorldComponentIndices indices)
{
    private readonly int index = indices[new EntComponent(typeof(T), typeof(N), false)];

    public int Index => index;
}
