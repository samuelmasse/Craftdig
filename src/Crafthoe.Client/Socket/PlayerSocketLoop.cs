namespace Crafthoe.Client;

[Player]
public class PlayerSocketLoop(
    PlayerNetLoop loop,
    PlayerSocket socket,
    PlayerPingReceiver pingReceiver,
    PlayerPongReceiver pongReceiver,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerChunkUpdateReceiver chunkUpdateReceiver,
    PlayerWorldIndicesUpdateReceiver worldIndicesUpdateReceiver,
    PlayerSlowDownReceiver slowDownReceiver,
    PlayerSlowTickReceiver slowTickReceiver)
{
    private Thread? thread;

    public void Start()
    {
        loop.Register<PingCommand>(pingReceiver.Receive);
        loop.Register<PongCommand>(pongReceiver.Receive);
        loop.Register<PositionUpdateCommand>(positionUpdateReceiver.Receive);
        loop.Register<ChunkUpdateCommand, byte>(chunkUpdateReceiver.Receive);
        loop.Register<WorldIndicesUpdateCommand, byte>(worldIndicesUpdateReceiver.Receive);
        loop.Register<SlowDownCommand>(slowDownReceiver.Receive);
        loop.Register<SlowTickCommand>(slowTickReceiver.Receive);

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
