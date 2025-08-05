namespace Crafthoe.Module;

[Module]
public class ModuleEntities
{
    private readonly Dictionary<string, EntObj> entities = [];
    private readonly List<Ent> list = [];

    public ReadOnlySpan<Ent> Span => CollectionsMarshal.AsSpan(list);

    public EntMut this[string name]
    {
        get
        {
            if (entities.TryGetValue(name, out var val))
                return (EntMut)val;

            var ent = new EntObj().ModuleName(name);
            entities.Add(name, ent);
            list.Add((Ent)ent);

            return (EntMut)ent;
        }
    }
}
