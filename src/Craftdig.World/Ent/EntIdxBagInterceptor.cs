namespace Craftdig.World;

public class EntIdxBagInterceptor<N>(EntIdxBagMut<N> bag)
{
    public void Intercept(EntMutIdx ent, bool value)
    {
        if (ent.Get<bool, N>() == value)
            return;

        if (value)
            bag.Add(ent);
        else bag.Remove(ent);
    }

    public void InterceptNoIndex(EntMutIdx ent, int value)
    {
        if (value == 0)
            bag.Remove(ent);
    }
}
