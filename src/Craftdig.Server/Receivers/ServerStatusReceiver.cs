namespace Craftdig.Server;

[Server]
public class ServerStatusReceiver(ServerConfig config, ServerSockets sockets)
{
    private byte[]? iconCache;
    private bool iconLoaded;

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

        if (config.Description != null)
            ns.Send<ServerDescriptionCommand, byte>(Encoding.UTF8.GetBytes(config.Description));

        var icon = GetIcon();
        if (icon != null)
            ns.Send<ServerIconCommand, byte>(icon);

        ns.Send<ServerStatusDoneCommand>();
    }

    private byte[]? GetIcon()
    {
        if (iconLoaded)
            return iconCache;

        var path = Path.Join(config.RootPath, "Icon.png");
        if (File.Exists(path))
            iconCache = File.ReadAllBytes(path);

        iconLoaded = true;
        return iconCache;
    }
}
