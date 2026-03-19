namespace Craftdig.World;

public class EntIdxContextBuilder
{
    private readonly EntObj ent = new();

    public EntObj Ent => ent;

    public void AddBag<N>(EntIdxBagMut<N> bag)
    {
        var inter = new EntIdxBagInterceptor<N>(bag);
        AddPost<bool, N>(inter.Intercept);
        AddPost<bool, WorldComponents.IsLoaded>(inter.Intercept);
        AddPre<int, EntIdxBagIndex<N>>(inter.InterceptNoIndex);
    }

    public void AddBagUnloaded<N>(EntIdxBagMut<N> bag)
    {
        var inter = new EntIdxBagInterceptorUnloaded<N>(bag);
        AddPost<bool, N>(inter.Intercept);
        AddPre<int, EntIdxBagIndex<N>>(inter.InterceptNoIndex);
    }

    public void AddPost<T, N>(Action<EntMutIdx> action) =>
        Add<EntIdxPost<T, N>, Action<EntMutIdx>>(action);

    public void AddPre<T, N>(Action<EntMutIdx, T> action) =>
        Add<EntIdxPre<T, N>, Action<EntMutIdx, T>>(action);

    public void AddPreDispose(Action<EntMutIdx> action) =>
        Add<EntIdxPreDispose, Action<EntMutIdx>>(action);

    public void Add<P, PT>(PT action)
    {
        var cur = ent.Get<ReadOnlyMemory<PT>, P>();
        var array = new PT[cur.Length + 1];
        cur.CopyTo(array);
        array[^1] = action;
        ent.Set<ReadOnlyMemory<PT>, P>(array);
    }
}
