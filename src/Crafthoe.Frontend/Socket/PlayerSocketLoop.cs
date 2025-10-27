namespace Crafthoe.Frontend;

[Player]
public class PlayerSocketLoop(
    NetLoop netLoop,
    NetEcho netEcho,
    PlayerSocket socket,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerChunkUpdateReceiver chunkUpdateReceiver,
    PlayerIndicesReceiver indicesReceiver)
{
    private bool stopping;

    public void Start()
    {
        netLoop.Register(NetEcho.Type, netEcho.Receive);
        netLoop.Register(WorldPositionUpdateWrapper.Type, positionUpdateReceiver.Receive);
        netLoop.Register(WorldChunkUpdateWrapper.Type, chunkUpdateReceiver.Receive);
        netLoop.Register(WorldIndicesWrapper.Type, indicesReceiver.Receive);
        new Thread(Loop).Start();
    }

    public void Stop()
    {
        stopping = true;
    }

    private void Loop()
    {
        try
        {
            netLoop.Run(socket);
        }
        catch
        {
            if (!stopping)
            {
                try
                {
                    socket.Raw.Disconnect(false);
                    socket.Raw.Dispose();
                }
                catch { }

                throw;
            }
        }
    }
}
