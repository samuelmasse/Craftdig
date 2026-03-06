namespace Craftdig.World;

[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntMutIdx : IEntMut
{
    public static implicit operator Ent(EntMutIdx a) => (Ent)a.ent;

    private readonly EntPtrIdx ent;

    public bool IsAlive => ent.IsAlive;
    public EntHandle Handle => ent.Handle;

    public EntMutIdx(EntPtrIdx ent) => this.ent = ent;

    public void Set<T, N>(in T value) => ent.Set<T, N>(value);
    public bool Unset<T, N>() => ent.Unset<T, N>();
    public T? Get<T, N>() => ent.Get<T, N>();
    public bool Has<T, N>() => ent.Has<T, N>();
    public override string ToString() => ent.ToString();
}
