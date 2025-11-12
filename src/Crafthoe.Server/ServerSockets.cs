namespace Crafthoe.Server;

[Server]
public class ServerSockets
{
    private readonly List<NetSocket> list = [];

    public void Add(NetSocket ns)
    {
        lock (this)
        {
            list.Add(ns);
        }
    }

    public void Remove(NetSocket ns)
    {
        lock (this)
        {
            list.Remove(ns);
        }
    }

    public void ForEach(Action<NetSocket> handler)
    {
        lock (this)
        {
            foreach (var item in list)
                handler(item);
        }
    }
}
