namespace Craftdig;

[Player]
public class PlayerEntUpdateReceiver(
    WorldEntSyncCatalog worldCatalog,
    DimensionEntSyncCatalog dimensionCatalog,
    PlayerEntUpdateQueue updates,
    PlayerSocket socket)
{
    public void Receive(EntSyncSchemaCommand command)
    {
        if (command.WorldSchema != worldCatalog.SchemaHash ||
            command.DimensionSchema != dimensionCatalog.SchemaHash)
            socket.Disconnect();
    }

    public void Receive(EntUpdateCommand command, ReadOnlySpan<byte> data)
        => updates.Enqueue(command, data);
}
