namespace Crafthoe.Client;

[Player]
public class PlayerSocketLoop(
    PlayerNetLoop loop,
    PlayerSocket socket,
    PlayerPingReceiver pingReceiver,
    PlayerPongReceiver pongReceiver,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerChunkUpdateReceiver chunkUpdateReceiver,
    PlayerWorldIndicesUpdateReceiver worldIndicesUpdateReceiver)
{
    private Thread? thread;

    public void Start()
    {
        loop.Register<PingCommand>(
            (int)CommonCommand.Ping, pingReceiver.Receive);

        loop.Register<PongCommand>(
            (int)CommonCommand.Pong, pongReceiver.Receive);

        loop.Register<PositionUpdateCommand>(
            (int)ClientCommand.PositionUpdate, positionUpdateReceiver.Receive);

        loop.Register<ChunkUpdateCommand, byte>(
            (int)ClientCommand.ChunkUpdate, chunkUpdateReceiver.Receive);

        loop.Register<byte>(
            (int)ClientCommand.WorldIndicesUpdate, worldIndicesUpdateReceiver.Receive);

        thread = new Thread(Loop);
        thread.Start();
    }

    public void Stop()
    {
        socket.Disconnect();
        thread?.Join();
    }

    private void Loop()
    {
        try
        {
            loop.Run(socket);
        }
        catch
        {
            if (socket.Connected)
                throw;
        }
    }
}
