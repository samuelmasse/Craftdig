namespace Crafthoe.Client;

[Player]
public class PlayerSocketLoop(
    NetLoop netLoop,
    NetEcho netEcho,
    PlayerSocket socket,
    PlayerPositionUpdateReceiver positionUpdateReceiver,
    PlayerChunkUpdateReceiver chunkUpdateReceiver,
    PlayerWorldIndicesUpdateReceiver worldIndicesUpdateReceiver)
{
    private bool stopping;

    public void Start()
    {
        netLoop.Register((int)CommonCommand.Echo, netEcho.Receive);
        netLoop.Register((int)ClientCommand.PositionUpdate, positionUpdateReceiver.Receive);
        netLoop.Register((int)ClientCommand.ChunkUpdate, chunkUpdateReceiver.Receive);
        netLoop.Register((int)ClientCommand.WorldIndicesUpdate, worldIndicesUpdateReceiver.Receive);
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
