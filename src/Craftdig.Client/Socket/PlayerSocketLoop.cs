namespace Craftdig.Client;

[Player]
public class PlayerSocketLoop(
    PlayerNetLoop loop,
    PlayerSocket socket,
    PlayerPingReceiver pingReceiver,
    PlayerPongReceiver pongReceiver,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerSectionUpdateReceiver sectionUpdateReceiver,
    PlayerChunkUpdateReceiver chunkUpdateReceiver,
    PlayerWorldIndicesUpdateReceiver worldIndicesUpdateReceiver,
    PlayerSlowTickReceiver slowTickReceiver)
{
    private Thread? thread;
    private Thread? pushThread;

    public void Start()
    {
        loop.Register<PingCommand>(pingReceiver.Receive);
        loop.Register<PongCommand>(pongReceiver.Receive);
        loop.Register<PositionUpdateCommand>(positionUpdateReceiver.Receive);
        loop.Register<ChunkUpdateCommand, byte>(chunkUpdateReceiver.Receive);
        loop.Register<SectionUpdateCommand, byte>(sectionUpdateReceiver.Receive);
        loop.Register<WorldIndicesUpdateCommand, byte>(worldIndicesUpdateReceiver.Receive);
        loop.Register<SlowTickCommand>(slowTickReceiver.Receive);

        thread = new Thread(Loop);
        thread.Start();

        pushThread = new Thread(Push);
        pushThread.Start();
    }

    public void Stop()
    {
        socket.Disconnect();
        thread?.Join();
        pushThread?.Join();
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

    private void Push()
    {
        try
        {
            socket.Push();
        }
        catch
        {
            if (socket.Connected)
                throw;
        }
    }
}
