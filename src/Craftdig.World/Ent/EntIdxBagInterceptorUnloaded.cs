namespace Craftdig.World;

public class EntIdxBagInterceptorUnloaded<N>(EntIdxBagMut<N> bag)
{
    public void Intercept(EntMutIdx ent)
    {
        var shouldBeInBag = ent.Get<bool, N>();

        if (bag.Contains(ent) == shouldBeInBag)
            return;

        if (shouldBeInBag)
            bag.Add(ent);
        else bag.Remove(ent);
    }

    public void InterceptNoIndex(EntMutIdx ent, int value)
    {
        if (value == 0)
            bag.Remove(ent);
    }
}
