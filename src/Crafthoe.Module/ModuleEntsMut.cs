namespace Crafthoe.Module;

[Module]
public class ModuleEntsMut
{
    private readonly Dictionary<string, EntObj> entities = [];
    private readonly List<EntMut> list = [];
    private readonly HashSet<EntMut> set = [];

    public ReadOnlySpan<EntMut> Span => CollectionsMarshal.AsSpan(list);
    public HashSet<EntMut> Set => set;

    public EntMut this[string name]
    {
        get
        {
            if (entities.TryGetValue(name, out var val))
                return (EntMut)val;

            var ent = new EntObj().ModuleName(name);
            entities.Add(name, ent);
            set.Add((EntMut)ent);
            list.Add((EntMut)ent);

            return (EntMut)ent;
        }
    }
}
