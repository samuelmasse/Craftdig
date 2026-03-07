namespace Craftdig.Module;

[Module]
public class ModuleEntsMut
{
    private readonly Dictionary<string, EntObj> ents = [];
    private readonly List<EntMut> list = [];
    private readonly HashSet<EntMut> set = [];

    public ReadOnlySpan<EntMut> Span => CollectionsMarshal.AsSpan(list);
    public HashSet<EntMut> Set => set;

    public EntMut this[string name]
    {
        get
        {
            if (ents.TryGetValue(name, out var val))
                return (EntMut)val;

            var ent = new EntObj() { ModuleName = name };
            ents.Add(name, ent);
            set.Add((EntMut)ent);
            list.Add((EntMut)ent);
            ent.RuntimeIndex = list.Count;

            return (EntMut)ent;
        }
    }

    public EntMut this[int runtimeIndex] => list[runtimeIndex - 1];

    public bool Contains(string name) => ents.ContainsKey(name);
    internal EntMut Get(string name) => (EntMut)ents[name];
}
