namespace Craftdig.World.Server;

[Components]
public interface IWorldServerComponents
{
    // Socket
    EntMutIdx SocketWorldPlayer { get; set; }
    int PlayerSlot { get; set; }

    // Ent synchronization
    bool IsWorldSyncPublished { get; set; }
    bool IsScratched { get; set; }
    ulong[]? ScratchedComponents { get; set; }

    // Socket synchronization
    bool IsWorldSyncReady { get; set; }

    // Inventory actions
    uint LastInventoryActionSequence { get; set; }
    InventoryActionStatus LastInventoryActionStatus { get; set; }
    long LastInventoryActionRevision { get; set; }
}
