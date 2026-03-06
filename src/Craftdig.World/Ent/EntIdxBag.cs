namespace Craftdig.World;

public class EntIdxBag<N>(EntIdxBagMut<N> bag)
{
    public ReadOnlySpan<EntMutIdx> Ents => bag.Ents;
}
