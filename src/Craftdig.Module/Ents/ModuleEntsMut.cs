namespace Craftdig;

[Module]
public class ModuleEntsMut
{
    private readonly EntArena arena = new();
    private readonly Dictionary<string, EntMut> ents = [];
    private readonly List<EntMut> list = [];
    private readonly HashSet<EntMut> set = [];

    public ReadOnlySpan<EntMut> Span => CollectionsMarshal.AsSpan(list);
    public HashSet<EntMut> Set => set;

    public EntMut this[string name]
    {
        get
        {
            if (ents.TryGetValue(name, out var val))
                return val;

            EntMut ent = arena.Alloc();
            ent.ModuleName = name;
            ents.Add(name, ent);
            set.Add(ent);
            list.Add(ent);
            ent.RuntimeIndex = list.Count;

            return ent;
        }
    }

    public EntMut this[int runtimeIndex] => list[runtimeIndex - 1];

    public bool Contains(string name) => ents.ContainsKey(name);
    internal EntMut Get(string name) => ents[name];

    public void Unload()
    {
        ents.Clear();
        set.Clear();
        list.Clear();
        arena.Dispose();
    }
}
