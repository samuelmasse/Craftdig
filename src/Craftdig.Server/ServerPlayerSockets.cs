namespace Craftdig.Dimension.Server;

[Server]
public class ServerPlayerSockets
{
    private readonly List<NetSocket> sockets = [];

    public ReadOnlySpan<NetSocket> Span => CollectionsMarshal.AsSpan(sockets);

    public void Add(NetSocket ns) => sockets.Add(ns);
    public void Remove(NetSocket ns) => sockets.Remove(ns);
}
