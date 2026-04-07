namespace Craftdig.Server;

[Server]
public class ServerStatusReceiver(ServerConfig config, ServerSockets sockets)
{
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

        ns.Send<ServerStatusDoneCommand>();
    }
}
