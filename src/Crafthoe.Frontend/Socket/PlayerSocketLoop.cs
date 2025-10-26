namespace Crafthoe.Frontend;

[Player]
public class PlayerSocketLoop(NetLoop netLoop, NetEcho netEcho, PlayerSocket socket)
{
    private bool stopping;

    public void Start()
    {
        netLoop.Register(NetEcho.Type, netEcho.Receive);
        new Thread(Loop).Start();

        socket.Socket.Send(netEcho.Wrap("Hello this is the client"));
        socket.Socket.Send(netEcho.Wrap("Please give me some chunks"));
    }

    public void Stop()
    {
        stopping = true;
    }

    private void Loop()
    {
        try
        {
            netLoop.Run(socket.Socket);
        }
        catch
        {
            if (!stopping)
                throw;
        }
    }
}
