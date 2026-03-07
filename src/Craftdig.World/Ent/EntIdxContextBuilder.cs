namespace Craftdig.World;

public class EntIdxContextBuilder
{
    private readonly EntObj ent = new EntObj().Mutate().ContextEntLiveSet([]);

    public EntObj Ent => ent;

    public void AddBag<N>(EntIdxBagMut<N> bag)
    {
        var inter = new EntIdxBagInterceptor<N>(bag);
        AddInterceptor<bool, N>(inter.Intercept);
        AddInterceptor<int, EntIdxBagIndex<N>>(inter.InterceptNoIndex);
    }

    public void AddInterceptor<T, N>(Action<EntMutIdx, T> action)
    {
        var cur = ent.Get<ReadOnlyMemory<Action<EntMutIdx, T>>, EntIdxInterceptorFor<T, N>>();
        var array = new Action<EntMutIdx, T>[cur.Length + 1];
        cur.CopyTo(array);
        array[^1] = action;
        ent.Set<ReadOnlyMemory<Action<EntMutIdx, T>>, EntIdxInterceptorFor<T, N>>(array);
    }
}
