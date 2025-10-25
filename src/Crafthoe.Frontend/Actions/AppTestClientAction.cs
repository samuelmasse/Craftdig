namespace Crafthoe.Frontend;

[App]
public class AppTestClientAction
{
    public void Run()
    {
        return;
        Console.WriteLine("Connecting");
        var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        var ns = new NetSocket(s);
        var addr = IPAddress.Parse("127.0.0.1");
        int port = 8080;
        s.Connect(addr, port);
        Console.WriteLine("Connected");

        var nloop = new NetLoop();
        var necho = new NetEcho();
        nloop.Register(NetEcho.Type, necho.Receive);
        new Thread(() => nloop.Run(ns)).Start();

        ns.Send(necho.Wrap("Hello this is the client"));
        ns.Send(necho.Wrap("Please give me some chunks"));
    }
}
