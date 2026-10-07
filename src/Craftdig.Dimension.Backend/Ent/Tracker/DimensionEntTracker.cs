namespace Craftdig;

[Dimension]
public class DimensionEntTracker(
    DimensionIndexedComponents indexedComponents,
    DimensionScope scope,
    DimensionEntIdxContext context)
{
    public void Register()
    {
        foreach (var component in indexedComponents.Components)
            StartTracking(component);
    }

    private void StartTracking(EntComponent component)
    {
        var type = component.ValueType.IsArray ?
            typeof(DimensionComponentArrayTracker<,>).MakeGenericType(component.ValueType.GetElementType()!, component.NameType) :
            typeof(DimensionComponentTracker<,>).MakeGenericType(component.ValueType, component.NameType);

        var tracker = (WorldComponentTracker)scope.New(type)!;
        tracker.AddTo(context);
    }
}
