namespace Craftdig.World.Backend;

[World]
public class WorldComponentIndicesFile
{
    private readonly string file;
    private readonly Dictionary<string, int> names;
    private readonly List<string> lines;

    public WorldComponentIndicesFile(WorldPaths paths)
    {
        file = Path.Join(paths.Root, "Components.txt");
        names = [];

        if (!File.Exists(file))
        {
            lines = [string.Empty];
            return;
        }

        lines = [.. File.ReadAllLines(file)];
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line))
                continue;

            names.Add(line, i);
        }
    }

    public int GetOrAdd(string name)
    {
        if (names.TryGetValue(name, out int val))
            return val;

        val = lines.Count;
        lines.Add(name);

        if (File.Exists(file))
            File.AppendAllLines(file, [name]);
        else File.WriteAllLines(file, lines);

        return val;
    }
}
