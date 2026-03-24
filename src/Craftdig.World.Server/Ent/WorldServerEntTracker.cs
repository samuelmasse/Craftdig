namespace Craftdig.World.Server;

[World]
public partial class WorldServerEntTracker(
    WorldIndexedComponents indexedComponents,
    WorldEntIdxContextBuilder context,
    WorldScope scope)
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
            typeof(WorldServerComponentArrayTracker<,>).MakeGenericType(component.ValueType.GetElementType()!, component.NameType) :
            typeof(WorldServerComponentTracker<,>).MakeGenericType(component.ValueType, component.NameType);

        var tracker = (WorldServerComponentTracker)scope.New(type)!;
        tracker.AddTo(context);
    }
}
