namespace Craftdig.World.Server;

[World]
public partial class WorldServerEntTracker(
    WorldEntSyncCatalog catalog,
    WorldEntIdxContextBuilder context,
    WorldScope scope)
{
    public void Run()
    {
        foreach (var component in catalog.Components)
            StartTracking(component.Component);
    }

    private void StartTracking(EntComponent component)
    {
        var type = component.ValueType.IsArray ?
            typeof(WorldServerComponentArrayTracker<,>).MakeGenericType(component.ValueType.GetElementType()!, component.NameType) :
            typeof(WorldServerComponentTracker<,>).MakeGenericType(component.ValueType, component.NameType);

        var tracker = (WorldServerComponentTracker)scope.New(type)!;
        tracker.AddTo(context);
    }
}
