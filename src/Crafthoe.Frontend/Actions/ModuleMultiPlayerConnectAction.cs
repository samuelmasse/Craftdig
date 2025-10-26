namespace Crafthoe.Frontend;

[Module]
public class ModuleMultiPlayerConnectAction
{
    public void Run(string host, int port)
    {
        Console.WriteLine("Connecting");
        var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        var ns = new NetSocket(s);
        s.Connect(host, port);
        Console.WriteLine("Connected");

        var nloop = new NetLoop();
        var necho = new NetEcho();
        nloop.Register(NetEcho.Type, necho.Receive);
        new Thread(() => nloop.Run(ns)).Start();

        ns.Send(necho.Wrap("Hello this is the client"));
        ns.Send(necho.Wrap("Please give me some chunks"));
    }
}
