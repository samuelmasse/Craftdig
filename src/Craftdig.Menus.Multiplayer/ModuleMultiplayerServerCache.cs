namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerServerCache(AppPaths paths)
{
    private string CacheDir => Path.Combine(paths.GamePath, "ServerCache");

    private string ServerDir(ServerAddress address) =>
        Path.Combine(CacheDir, $"{address.Host}_{address.Port}");

    public void Save(ServerAddress address, ServerPingResult result)
    {
        if (!result.Success)
            return;

        var dir = ServerDir(address);
        Directory.CreateDirectory(dir);

        var descPath = Path.Combine(dir, "Description.txt");
        if (result.Description != null)
            File.WriteAllText(descPath, result.Description);
        else if (File.Exists(descPath))
            File.Delete(descPath);

        var iconPath = Path.Combine(dir, "Icon.png");
        if (result.IconData != null)
            File.WriteAllBytes(iconPath, result.IconData);
        else if (File.Exists(iconPath))
            File.Delete(iconPath);
    }

    public void Prune(ReadOnlySpan<ServerEntry> servers)
    {
        if (!Directory.Exists(CacheDir))
            return;

        var valid = new HashSet<string>();
        foreach (var server in servers)
            valid.Add(ServerDir(server.Address));

        foreach (var dir in Directory.GetDirectories(CacheDir))
        {
            if (!valid.Contains(dir))
                Directory.Delete(dir, true);
        }
    }

    public string? LoadDescription(ServerAddress address)
    {
        var path = Path.Combine(ServerDir(address), "Description.txt");
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    public byte[]? LoadIcon(ServerAddress address)
    {
        var path = Path.Combine(ServerDir(address), "Icon.png");
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }
}
