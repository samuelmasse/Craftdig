namespace Craftdig;

internal sealed class ConnectedPair : IDisposable
{
    private readonly TcpListener listener;
    private readonly TcpClient client;
    private readonly TcpClient server;

    private ConnectedPair(TcpListener listener, TcpClient client, TcpClient server, NetSocket socket)
    {
        this.listener = listener;
        this.client = client;
        this.server = server;
        Server = socket;
    }

    public readonly NetSocket Server;
    public NetworkStream ClientStream => client.GetStream();

    public static ConnectedPair Create()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var client = new TcpClient();
        client.Connect(endpoint);
        var server = listener.AcceptTcpClient();
        var log = new LogRuntime(TextWriter.Null).Log;
        return new(listener, client, server, new(log, server, server.GetStream()));
    }

    public void Dispose()
    {
        Server.Disconnect();
        server.Dispose();
        client.Dispose();
        listener.Dispose();
    }
}
