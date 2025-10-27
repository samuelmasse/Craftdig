namespace Crafthoe.Server;

[World]
public class WorldListener(NetLoop nloop)
{
    public void Start()
    {
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var addr = IPAddress.Parse("127.0.0.1");
        int port = 8080;

        listener.Bind(new IPEndPoint(addr, port));
        listener.Listen(10);

        Console.WriteLine($"Listening on {addr}:{port}...");

        while (true)
        {
            var s = listener.Accept();
            s.NoDelay = true;

            try
            {
                new Thread(() => ClientLoop(new(s))).Start();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Error in listener");
                Console.Error.WriteLine(e);
                Console.ResetColor();
            }
        }
    }

    private void ClientLoop(NetSocket ns)
    {
        Console.WriteLine($"Socket connected");

        try
        {
            nloop.Run(ns);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Error in socket");
            Console.Error.WriteLine(e);
            Console.ResetColor();
        }

        Console.WriteLine($"Socket disconnected");
    }
}
