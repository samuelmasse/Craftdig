namespace Craftdig;

[World]
public class WorldEntTracker(
    WorldIndexedComponents indexedComponents,
    WorldScope scope,
    WorldEntIdxContext context)
{
    public void Register()
    {
        foreach (var component in indexedComponents.Components)
            StartTracking(component);
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
