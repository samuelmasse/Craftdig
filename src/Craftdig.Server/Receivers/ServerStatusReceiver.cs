namespace Craftdig.Server;

[Server]
public class ServerStatusReceiver(AppLog log, ServerConfig config, ServerSockets sockets)
{
    private byte[]? iconCache;
    private bool iconLoaded;
    private long descriptionHash;
    private long iconHash;

    public void Receive(NetSocket ns, ServerStatusCommand cmd)
    {
        ns.Send(new PongCommand { Ping = new PingCommand { Timestamp = cmd.Timestamp } });

        int currentPlayers = 0;
        sockets.ForEach(s =>
        {
            if (s.Connected && s.IsAuthenticated)
                currentPlayers++;
        });

        ns.Send(new ServerPopulationCommand()
        {
            MaxPlayers = config.MaxPlayers,
            CurrentPlayers = currentPlayers
        });

        if (config.Description != null && cmd.DescriptionHash != GetDescriptionHash())
            ns.Send<ServerDescriptionCommand, byte>(Encoding.UTF8.GetBytes(config.Description));

        var icon = GetIcon();
        if (icon != null && cmd.IconHash != iconHash)
            ns.Send<ServerIconCommand, byte>(icon);

        ns.Send<ServerStatusDoneCommand>();
    }

    private long GetDescriptionHash()
    {
        if (descriptionHash == 0 && config.Description != null)
            descriptionHash = ContentHash(Encoding.UTF8.GetBytes(config.Description));

        return descriptionHash;
    }

    private byte[]? GetIcon()
    {
        if (iconLoaded)
            return iconCache;

        var path = Path.Join(config.RootPath, "Icon.png");
        if (File.Exists(path))
        {
            iconCache = File.ReadAllBytes(path);
            iconHash = ContentHash(iconCache);
        }

        iconLoaded = true;
        return iconCache;
    }

    private static long ContentHash(byte[] data) =>
        BinaryPrimitives.ReadInt64LittleEndian(SHA256.HashData(data));
}
