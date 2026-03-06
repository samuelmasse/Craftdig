namespace Craftdig.Player.Frontend;

[Player]
[DebuggerTypeProxy(typeof(EntDebugView))]
public class PlayerEnt(EntMutIdx ent) : IEntMut
{
    private readonly EntMutIdx ent = ent;

    public static implicit operator Ent(PlayerEnt a) => (Ent)a.ent;
    public static implicit operator EntMutIdx(PlayerEnt a) => a.ent;

    public EntHandle Handle => ent.Handle;
    public bool IsAlive => ent.IsAlive;
    public bool Has<T, N>() => ent.Has<T, N>();
    public T? Get<T, N>() => ent.Get<T, N>();
    public void Set<T, N>(in T value) => ent.Set<T, N>(value);
    public bool Unset<T, N>() => ent.Unset<T, N>();
    public override string ToString() => ent.ToString();
}
