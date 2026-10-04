namespace Craftdig;

[Server]
public class ServerSockets
{
    private readonly List<ServerSocketConnection> list = [];
    private readonly List<ServerSocketConnection> completed = [];

    public void Add(ServerSocketConnection connection)
    {
        lock (this)
        {
            list.Add(connection);
            completed.EnsureCapacity(list.Count);
        }
    }

    // Capture before dimension queues run: completed I/O cannot enqueue another request.
    public void CollectCompleted()
    {
        lock (this)
        {
            foreach (var connection in list)
            {
                if (connection.IsCompleted && connection.Socket.PresenceConnection?.IsReleased != false)
                    completed.Add(connection);
            }
        }
    }

    public void ReleaseCompleted()
    {
        lock (this)
        {
            foreach (var connection in completed)
            {
                list.Remove(connection);
                connection.Socket.ReleaseState();
            }

            completed.Clear();
        }
    }

    public void ForEach(Action<NetSocket> handler)
    {
        lock (this)
        {
            foreach (var item in list)
            {
                if (item.Socket.Connected)
                    handler(item.Socket);
            }
        }
    }
}
