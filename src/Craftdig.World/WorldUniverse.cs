namespace Craftdig;

[World]
[DebuggerTypeProxy(typeof(EntDebugView))]
public class WorldUniverse : IEntMut
{
    private EntMutIdx ent;

    public static implicit operator Ent(WorldUniverse universe) => universe.ent;
    public static implicit operator EntMutIdx(WorldUniverse universe) => universe.ent;

    public void Bind(EntMutIdx value) => ent = value;

    public EntHandle Handle => ent.Handle;
    public bool IsAlive => ent.IsAlive;
    public bool Has<T, N>() => ent.Has<T, N>();
    public T? Get<T, N>() => ent.Get<T, N>();
    public void Set<T, N>(in T value) => ent.Set<T, N>(value);
    public bool Unset<T, N>() => ent.Unset<T, N>();
    public override string ToString() => ent.ToString();
}
