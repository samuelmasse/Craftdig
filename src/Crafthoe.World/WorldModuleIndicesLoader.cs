namespace Crafthoe.World;

[World]
public class WorldModuleIndicesLoader(ModuleEnts ents, WorldScope scope, WorldPaths paths)
{
    public void Run()
    {
        var indices = new int[ents.Span.Length + 1];
        var names = new List<string>();
        var dict = new Dictionary<string, int>();
        var file = Path.Join(paths.Root, "Indices.txt");
        bool changed = false;

        if (File.Exists(file))
        {
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                dict.Add(line, names.Count);
                names.Add(line);
            }
        }
        else names.Add(string.Empty);

        foreach (var ent in ents.Span)
        {
            if (dict.TryGetValue(ent.ModuleName(), out int index))
                indices[ent.RuntimeIndex()] = index;
            else
            {
                indices[ent.RuntimeIndex()] = names.Count;
                names.Add(ent.ModuleName());
                changed = true;
            }
        }

        if (changed)
            File.WriteAllLines(file, [.. names]);

        var rindices = new int[names.Count];
        for (int i = 0; i < names.Count; i++)
        {
            var name = names[i];
            if (ents.Contains(name))
                rindices[i] = ents[name].RuntimeIndex();
        }

        scope.Add(new WorldModuleIndices(ents, indices, rindices));
    }
}
