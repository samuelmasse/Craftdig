namespace Craftdig;

[Dimension]
public class DimensionServerEntDisposeTracker(
    DimensionSyncChunks chunks,
    DimensionSockets sockets,
    DimensionEntDisposals disposals)
{
    public void InterceptDispose(EntMutIdx ent)
    {
        NetSocket? owner = null;
        foreach (var socket in sockets.Span)
        {
            if ((Ent)socket.SocketPlayer == ent)
            {
                owner = socket;
                break;
            }
        }

        chunks.Remove(ent, ent.SyncCloc);
        disposals.Add(ent, owner);
    }
}
