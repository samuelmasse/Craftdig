namespace Craftdig.Dimension.Backend;

[Dimension]
public class DimensionEntTracker(WorldIndexedComponents indexedComponents, DimensionScope scope, DimensionEntIdxContextBuilder context)
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
            typeof(TrackerArray<,>).MakeGenericType(component.ValueType.GetElementType()!, component.NameType) :
            typeof(Tracker<,>).MakeGenericType(component.ValueType, component.NameType);

        var tracker = (Tracker)scope.New(type)!;
        tracker.AddTo(context);
    }

    private abstract class Tracker
    {
        public abstract void AddTo(EntIdxContextBuilder context);
    }

    [Dimension]
    private class Tracker<T, N>(AppLog log) : Tracker where T : IEquatable<T>
    {
        public override void AddTo(EntIdxContextBuilder context) => context.AddPre<T, N>(Intercept);

        private void Intercept(EntMutIdx ent, T value)
        {
            var old = ent.Get<T, N>();
            if (value.Equals(old))
                return;

            log.Info("New value {0} ({1} -> {2}) for {3}", typeof(N).Name, old, value, ent.Id);
        }
    }

    [Dimension]
    private class TrackerArray<T, N>(AppLog log) : Tracker where T : IEquatable<T>
    {
        public override void AddTo(EntIdxContextBuilder context) => context.AddPre<T[], N>(Intercept);

        private void Intercept(EntMutIdx ent, T[] value)
        {
            log.Info("New array {0} for {1}", typeof(N).Name, ent.Id);
        }
    }
}
