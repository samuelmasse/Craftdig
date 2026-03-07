namespace Craftdig.World.Backend;

[World]
public class WorldIndexedComponentsMut(WorldComponentIndicesFile componentIndicesFile, WorldComponentIndicesMut componentIndices)
{
    private readonly List<EntComponent> list = [];
    private readonly HashSet<EntComponent> set = [];

    public ReadOnlySpan<EntComponent> Components => CollectionsMarshal.AsSpan(list);

    public void Add(EntComponent component)
    {
        if (set.Add(component))
        {
            list.Add(component);
            int index = componentIndicesFile.GetOrAdd(component.NameType.Name);
            componentIndices.Add(component, index);
        }
    }

    public void AddSaved<T>() where T : IComponentGroup
    {
        var groupType = typeof(T);
        var interaceType = T.SourceInterfaceType;

        var componentTypes = groupType.GetNestedTypes(BindingFlags.Public)
            .Where(t => typeof(IComponent).IsAssignableFrom(t))
            .ToArray();

        var interfaceSavedProperties = interaceType.GetProperties()
            .Where(p => p.IsDefined(typeof(SavedAttribute)))
            .Select(p => p.Name)
            .ToHashSet();

        var savedComponents = componentTypes
            .Where(c => interfaceSavedProperties.Contains(c.Name))
            .Select(c => (EntComponent)c.GetProperty(nameof(IComponent.Component))?.GetValue(null)!)
            .ToList();

        savedComponents.ForEach(Add);
    }
}

[World]
public class WorldIndexedComponents(WorldIndexedComponentsMut indexedComponents)
{
    public ReadOnlySpan<EntComponent> Components => indexedComponents.Components;
}
