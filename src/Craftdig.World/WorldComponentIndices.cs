namespace Craftdig;

[World]
public class WorldComponentIndicesMut
{
    private readonly Dictionary<EntComponent, int> dict = [];
    private readonly Dictionary<int, EntComponent> rdict = [];

    public int this[EntComponent index] => dict[index];
    public EntComponent this[int index] => rdict[index];

    public bool TryGet(int index, out EntComponent component) => rdict.TryGetValue(index, out component);

    public void Add(EntComponent component, int index)
    {
        dict.Add(component, index);
        rdict.Add(index, component);
    }
}

[World]
public class WorldComponentIndices(WorldComponentIndicesMut indices)
{
    public int this[EntComponent index] => indices[index];
    public EntComponent this[int index] => indices[index];
    public bool TryGet(int index, out EntComponent component) => indices.TryGet(index, out component);
}
