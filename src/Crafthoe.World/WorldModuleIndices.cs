namespace Crafthoe.World;

[World]
public class WorldModuleIndices(ModuleEnts ents)
{
    private string[] names = [];
    private int[] rtToIndex = [];
    private int[] indexToRt = [];

    public ReadOnlySpan<string> Names => names;
    public int this[Ent block] => rtToIndex[block.RuntimeIndex()];
    public Ent this[int index] => ents[indexToRt[index]];

    public void Apply(string[] names)
    {
        this.names = names;

        var dict = new Dictionary<string, int>();
        for (int i = 0; i < names.Length; i++)
            dict.Add(names[i], i);

        rtToIndex = new int[ents.Span.Length + 1];
        indexToRt = new int[names.Length];

        foreach (var ent in ents.Span)
        {
            if (dict.TryGetValue(ent.ModuleName(), out int index))
                rtToIndex[ent.RuntimeIndex()] = index;
        }

        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i];
            if (ents.Contains(name))
                indexToRt[i] = ents[name].RuntimeIndex();
        }
    }
}
