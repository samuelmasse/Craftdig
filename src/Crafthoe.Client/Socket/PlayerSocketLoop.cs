namespace Crafthoe.Client;

[Player]
public class PlayerSocketLoop(
    NetLoop netLoop,
    NetEcho netEcho,
    PlayerSocket socket,
    PlayerPingReceiver pingReceiver,
    PlayerPongReceiver pongReceiver,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerChunkUpdateReceiver chunkUpdateReceiver,
    PlayerWorldIndicesUpdateReceiver worldIndicesUpdateReceiver)
{
    public void Start()
    {
        netLoop.Register((int)CommonCommand.Echo, netEcho.Receive);
        netLoop.Register((int)CommonCommand.Ping, pingReceiver.Receive);
        netLoop.Register((int)CommonCommand.Pong, pongReceiver.Receive);
        netLoop.Register((int)ClientCommand.PositionUpdate, positionUpdateReceiver.Receive);
        netLoop.Register((int)ClientCommand.ChunkUpdate, chunkUpdateReceiver.Receive);
        netLoop.Register((int)ClientCommand.WorldIndicesUpdate, worldIndicesUpdateReceiver.Receive);
        new Thread(Loop).Start();
    }

    public void Stop()
    {
        try { socket.Disconnect(); } catch { }
    }

    private void Loop()
    {
        try
        {
            netLoop.Run(socket);
        }
        catch
        {
            if (socket.Connected)
                throw;
        }
    }
}
