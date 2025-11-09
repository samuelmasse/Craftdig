namespace Crafthoe.Server;

[Server]
public class ServerClientLoop(NetLoop nloop)
{
    public void Start(NetSocket socket)
    {
        new Thread(() => Loop(socket)).Start();
    }

    private void Loop(NetSocket socket)
    {
        Console.WriteLine($"Socket connected");

        try
        {
            nloop.Run(socket);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Error in socket");
            Console.Error.WriteLine(e);
        }

        socket.Disconnect();

        Console.WriteLine($"Socket disconnected");
    }
}
