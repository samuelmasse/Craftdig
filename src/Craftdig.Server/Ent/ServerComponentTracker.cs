namespace Craftdig.Server;

public abstract class ServerComponentTracker
{
    public abstract void AddTo(EntIdxContextBuilder context);
}

[Server]
public class ServerComponentTracker<T, N>(ServerEntScratched scratched, WorldComponentIndex<T, N> index) :
    ServerComponentTracker where T : IEquatable<T>
{
    public override void AddTo(EntIdxContextBuilder context) => context.AddPre<T, N>(Intercept);

    private void Intercept(EntMutIdx ent, T value)
    {
        var old = ent.Get<T, N>();
        if (value.Equals(old))
            return;

        scratched.Mark(ent, index.Index);
    }
}

[Server]
public class ServerComponentArrayTracker<T, N>(ServerEntScratched scratched, WorldComponentIndex<T[], N> index) :
    ServerComponentTracker where T : IEquatable<T>
{
    public override void AddTo(EntIdxContextBuilder context) => context.AddPre<T[], N>(Intercept);

    private void Intercept(EntMutIdx ent, T[] value)
    {
        scratched.Mark(ent, index.Index);
    }
}
