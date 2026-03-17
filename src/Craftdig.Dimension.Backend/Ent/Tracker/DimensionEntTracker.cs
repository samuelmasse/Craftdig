namespace Craftdig.Dimension.Backend;

[Dimension]
public partial class DimensionEntTracker(
    WorldIndexedComponents indexedComponents,
    DimensionScope scope,
    DimensionEntIdxContextBuilder context)
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
            typeof(DimensionComponentArrayTracker<,>).MakeGenericType(component.ValueType.GetElementType()!, component.NameType) :
            typeof(DimensionComponentTracker<,>).MakeGenericType(component.ValueType, component.NameType);

        var tracker = (DimensionComponentTracker)scope.New(type)!;
        tracker.AddTo(context);
    }
}
