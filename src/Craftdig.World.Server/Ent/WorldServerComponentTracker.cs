namespace Craftdig.World.Server;

public abstract class WorldServerComponentTracker
{
    public abstract void AddTo(EntIdxContextBuilder context);
}

[World]
public class WorldServerComponentTracker<T, N>(WorldEntScratched scratched, WorldComponentIndex<T, N> index) :
    WorldServerComponentTracker where T : IEquatable<T>
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

[World]
public class WorldServerComponentArrayTracker<T, N>(WorldEntScratched scratched, WorldComponentIndex<T[], N> index) :
    WorldServerComponentTracker where T : IEquatable<T>
{
    public override void AddTo(EntIdxContextBuilder context) => context.AddPre<T[], N>(Intercept);

    private void Intercept(EntMutIdx ent, T[] value)
    {
        scratched.Mark(ent, index.Index);
    }
}
