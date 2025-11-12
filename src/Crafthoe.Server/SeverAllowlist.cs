namespace Crafthoe.Server;

[Server]
public class SeverAllowlist(ServerPaths paths, ServerDefaults defaults)
{
    private readonly string file = Path.Join(paths.Root, "Allowlist.txt");
    private HashSet<string>? set;

    public void Allow(string email)
    {
        lock (this)
        {
            if (set == null)
                Read();

            if (set!.Contains(email))
                return;
            else throw new Exception();
        }
    }

    private void Read()
    {
        if (!File.Exists(file))
            Persist();

        var lines = File.ReadAllLines(file);

        set = [];

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line))
                continue;

            set.Add(line);
        }
    }

    private void Persist()
    {
        if (set == null)
        {
            set = [];
            foreach (var item in defaults.Allowlist)
                set.Add(item);
        }

        var arr = set.ToArray();
        Array.Sort(arr);
        File.WriteAllLines(file, arr);
    }
}
