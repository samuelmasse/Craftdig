namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerServerList(AppPaths paths)
{
    private readonly string file = Path.Join(paths.GamePath, "Servers.json");
    private readonly List<ServerEntry> servers = [];
    private bool loaded;

    public ReadOnlySpan<ServerEntry> Servers
    {
        get
        {
            Load();
            return CollectionsMarshal.AsSpan(servers);
        }
    }

    public void Add(ServerEntry entry)
    {
        Load();
        servers.Add(entry);
        Save();
    }

    public void Remove(ServerEntry entry)
    {
        Load();
        servers.Remove(entry);
        Save();
    }

    public void Edit(ServerEntry old, ServerEntry updated)
    {
        Load();
        var index = servers.IndexOf(old);
        if (index >= 0)
            servers[index] = updated;
        Save();
    }

    private void Load()
    {
        if (loaded)
            return;

        loaded = true;

        if (!File.Exists(file))
            return;

        try
        {
            var json = File.ReadAllText(file);
            var list = JsonSerializer.Deserialize<List<ServerEntry>>(json) ?? [];
            servers.AddRange(list);
        }
        catch { }
    }

    private void Save()
    {
        var dir = Path.GetDirectoryName(file)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(file, JsonSerializer.Serialize(servers, new JsonSerializerOptions { WriteIndented = true }));
    }
}
