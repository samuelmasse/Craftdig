namespace Crafthoe.Module;

[Module]
public class ModuleEnts(ModuleEntsMut entsMut)
{
    private readonly HashSet<EntMut> set = [];
    private Ent[] array = [default];
    private int count;

    public ReadOnlySpan<Ent> Span
    {
        get
        {
            Refresh();
            return array.AsSpan()[..count];
        }
    }

    public HashSet<EntMut> Set
    {
        get
        {
            Refresh();
            return set;
        }
    }

    public Ent this[string name] => entsMut[name];

    private void Refresh()
    {
        var mut = entsMut.Span;

        if (count == mut.Length)
            return;

        if (array.Length <= mut.Length)
            Array.Resize(ref array, (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)mut.Length));

        for (int i = 0; i < mut.Length; i++)
        {
            array[i] = mut[i];
            set.Add(mut[i]);
        }

        count = mut.Length;
    }
}
