namespace Craftdig.World;

[DebuggerTypeProxy(typeof(EntDebugView))]
public readonly record struct EntPtrIdx : IEntMut, IDisposable
{
    public static implicit operator EntMutIdx(EntPtrIdx a) => new(a);
    public static implicit operator Ent(EntPtrIdx a) => (Ent)a.ent;

    private readonly EntPtr ent;
    private readonly Ent context;

    public bool IsAlive => ent.IsAlive;
    public EntHandle Handle => ent.Handle;

    public EntPtrIdx(EntPtr ent, Ent context)
    {
        this.ent = ent;
        this.context = context;
    }

    public void Set<T, N>(in T value)
    {
        var interceptors = context.Get<ReadOnlyMemory<Action<EntMutIdx, T>>, EntIdxInterceptorFor<T, N>>();

        foreach (var interceptor in interceptors.Span)
            interceptor(this, value);

        ent.Set<T, N>(value);
    }

    public bool Unset<T, N>()
    {
        Set<T, N>(default!);
        return ent.Unset<T, N>();
    }

    public void Dispose()
    {
        this.Clear();
        ent.Dispose();
        context.ContextEntLiveSet.Remove(this);
    }

    public T? Get<T, N>() => ent.Get<T, N>();
    public bool Has<T, N>() => ent.Has<T, N>();
    public override string ToString() => ent.ToString();
}
