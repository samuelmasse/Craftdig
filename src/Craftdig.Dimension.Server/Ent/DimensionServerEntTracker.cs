namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionServerEntTracker(
    DimensionEntSyncCatalog catalog,
    DimensionEntIdxContextBuilder context,
    DimensionScope scope)
{
    public void Run()
    {
        foreach (var component in catalog.Components)
            StartTracking(component.Component);
    }

    private void StartTracking(EntComponent component)
    {
        var type = component.ValueType.IsArray ?
            typeof(DimensionServerComponentArrayTracker<,>).MakeGenericType(
                component.ValueType.GetElementType()!,
                component.NameType) :
            typeof(DimensionServerComponentTracker<,>).MakeGenericType(
                component.ValueType,
                component.NameType);

        var tracker = (WorldServerComponentTracker)scope.New(type)!;
        tracker.AddTo(context);
    }
}
