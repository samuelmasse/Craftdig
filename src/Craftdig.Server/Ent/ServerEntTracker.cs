namespace Craftdig.Server;

[Server]
public partial class ServerEntTracker(
    WorldIndexedComponents indexedComponents,
    WorldEntIdxContextBuilder context,
    ServerScope scope)
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
            typeof(ServerComponentArrayTracker<,>).MakeGenericType(component.ValueType.GetElementType()!, component.NameType) :
            typeof(ServerComponentTracker<,>).MakeGenericType(component.ValueType, component.NameType);

        var tracker = (ServerComponentTracker)scope.New(type)!;
        tracker.AddTo(context);
    }
}
