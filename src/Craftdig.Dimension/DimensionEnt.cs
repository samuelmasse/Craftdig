namespace Craftdig.Dimension;

[Dimension]
[DebuggerTypeProxy(typeof(EntDebugView))]
public class DimensionEnt(Ent ent) : IEntRead
{
    private readonly Ent ent = ent;

    public static implicit operator Ent(DimensionEnt a) => a.ent;

    public EntHandle Handle => ent.Handle;
    public bool Has<T, N>() => ent.Has<T, N>();
    public T? Get<T, N>() => ent.Get<T, N>();
    public override string ToString() => ent.ToString();
}
