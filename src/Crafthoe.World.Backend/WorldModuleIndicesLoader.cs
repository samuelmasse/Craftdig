namespace Crafthoe.World.Backend;

[WorldLoader]
public class WorldModuleIndicesLoader(ModuleEnts ents, WorldPaths paths, WorldModuleIndices moduleIndices)
{
    public void Run()
    {
        var names = new List<string>();
        var set = new HashSet<string>();
        var file = Path.Join(paths.Root, "Indices.txt");
        bool changed = false;

        if (File.Exists(file))
        {
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                set.Add(line);
                names.Add(line);
            }
        }
        else names.Add(string.Empty);

        foreach (var ent in ents.Span)
        {
            if (!set.Contains(ent.ModuleName()))
            {
                names.Add(ent.ModuleName());
                changed = true;
            }
        }

        if (changed)
            File.WriteAllLines(file, [.. names]);

        moduleIndices.Apply(names.ToArray());
    }
}
