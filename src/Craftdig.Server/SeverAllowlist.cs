namespace Craftdig.Server;

[Server]
public class SeverAllowlist(ServerDefaults defaults, ServerConfig config)
{
    private readonly string file = Path.Join(config.RootPath, "Allowlist.txt");
    private HashSet<string>? set;

    public bool Allow(string email)
    {
        if (config.PublicServer)
            return true;

        lock (this)
        {
            if (set == null)
                Read();

            return set!.Contains(email);
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
