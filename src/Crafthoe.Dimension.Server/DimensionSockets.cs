namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionSockets
{
    private readonly List<NetSocket> sockets = [];

    public ReadOnlySpan<NetSocket> Span => CollectionsMarshal.AsSpan(sockets);

    public void Add(NetSocket ns) => sockets.Add(ns);
    public void Remove(NetSocket ns) => sockets.Remove(ns);
}
