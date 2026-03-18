namespace Craftdig.World.Backend;

[World]
public partial class WorldEntTracker(
    WorldIndexedComponents indexedComponents,
    WorldScope scope,
    WorldEntIdxContextBuilder context)
{
    private int index;

    public void Tick()
    {
        while (index < indexedComponents.Components.Length)
        {
            StartTracking(indexedComponents.Components[index]);
            index++;
        }
    }

    private void StartTracking(EntComponent component)
    {
        var type = component.ValueType.IsArray ?
            typeof(WorldComponentArrayTracker<,>).MakeGenericType(component.ValueType.GetElementType()!, component.NameType) :
            typeof(WorldComponentTracker<,>).MakeGenericType(component.ValueType, component.NameType);

        var tracker = (WorldComponentTracker)scope.New(type)!;
        tracker.AddTo(context);
    }
}
