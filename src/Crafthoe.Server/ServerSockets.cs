namespace Crafthoe.Server;

[Server]
public class ServerSockets
{
    private readonly HashSet<NetSocket> set = [];

    public void Add(NetSocket ns)
    {
        lock (this)
        {
            set.Add(ns);
        }
    }

    public void Remove(NetSocket ns)
    {
        lock (this)
        {
            set.Remove(ns);
        }
    }

    public List<NetSocket> ToList()
    {
        lock (this)
        {
            return [.. set];
        }
    }
}
