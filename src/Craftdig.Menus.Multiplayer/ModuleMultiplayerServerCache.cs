namespace Craftdig;

[Module]
public class ModuleMultiplayerServerCache(AppPaths paths)
{
    private string CacheDir => Path.Combine(paths.GamePath, "ServerCache");

    private string ServerDir(ServerAddress address) =>
        Path.Combine(CacheDir, $"{address.Host}_{address.Port}");

    private string DescriptionPath(ServerAddress address) =>
        Path.Combine(ServerDir(address), "Description.txt");

    private string IconPath(ServerAddress address) =>
        Path.Combine(ServerDir(address), "Icon.png");

    public void Save(ServerAddress address, ServerPingResult result)
    {
        if (!result.Success)
            return;

        var dir = ServerDir(address);
        Directory.CreateDirectory(dir);

        if (result.Description != null)
            File.WriteAllBytes(DescriptionPath(address), Encoding.UTF8.GetBytes(result.Description));
        if (result.IconData != null)
            File.WriteAllBytes(IconPath(address), result.IconData);
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

    public string? LoadDescription(ServerAddress address) =>
        LoadFile(DescriptionPath(address)) is { } bytes ? Encoding.UTF8.GetString(bytes) : null;

    public byte[]? LoadIcon(ServerAddress address) =>
        LoadFile(IconPath(address));

    public long DescriptionHash(ServerAddress address) =>
        HashFile(DescriptionPath(address));

    public long IconHash(ServerAddress address) =>
        HashFile(IconPath(address));

    private static byte[]? LoadFile(string path) =>
        File.Exists(path) ? File.ReadAllBytes(path) : null;

    private static long HashFile(string path) =>
        File.Exists(path) ? BinaryPrimitives.ReadInt64LittleEndian(SHA256.HashData(File.ReadAllBytes(path))) : 0;
}
