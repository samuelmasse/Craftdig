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

    /// <inheritdoc />
    public T? GetArchetypal<T, N, A>() => ent.GetArchetypal<T, N, A>();

    /// <inheritdoc />
    public bool HasArchetypal<T, N, A>() => ent.HasArchetypal<T, N, A>();

    /// <inheritdoc />
    public void SetArchetypal<T, N, A>(in T value) => ent.SetArchetypal<T, N, A>(in value);

    /// <inheritdoc />
    public bool UnsetArchetypal<T, N, A>() => ent.UnsetArchetypal<T, N, A>();

    public override string ToString() => ent.ToString();
}
