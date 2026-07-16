namespace Craftdig.Client;

[Player]
public class PlayerEntSync(
    PlayerEntUpdateValidator validator,
    PlayerEntUpdateApplier applier,
    PlayerEntReplicas replicas,
    PlayerEntUpdateQueue updates,
    PlayerSocket socket)
{
    public bool OwnerReady => replicas.OwnerReady;

    public void Frame()
    {
        while (updates.TryDequeue(out var update))
        {
            // Validate the entire packet before mutating the ECS. A malformed tail must not
            // leave the client with a partially applied authoritative update.
            bool valid = validator.Validate(update);
            if (valid)
                applier.Apply(update);

            updates.Return(update);
            if (!valid)
            {
                socket.Disconnect();
                updates.Clear();
                return;
            }
        }
    }

    public void ChunkLoaded(Vec2i chunk) => replicas.ChunkLoaded(chunk);

    public void ChunkUnloaded(Vec2i chunk) => replicas.ChunkUnloaded(chunk);
}
