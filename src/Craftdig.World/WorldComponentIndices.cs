namespace Craftdig.World;

[World]
public class WorldComponentIndicesMut
{
    private readonly Dictionary<EntComponent, int> dict = [];

    public int this[EntComponent index] => dict[index];

    public void Add(EntComponent component, int index) => dict.Add(component, index);
}

[World]
public class WorldComponentIndices(WorldComponentIndicesMut indices)
{
    public int this[EntComponent index] => indices[index];
}
