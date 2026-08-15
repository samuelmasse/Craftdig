namespace Craftdig;

[Components]
public interface IDimensionServerComponents
{
    // Socket
    EntMutIdx SocketPlayer { get; set; }
    HashSet<Vec2i>? SocketStreamedChunks { get; set; }
    HashSet<Vec3i>? SocketForgottenSections { get; set; }
    Queue<Vec3i>? SocketForgottenSectionQueue { get; set; }
    PlayerSpawnPhase PlayerSpawnPhase { get; set; }
    Vec2i TerrainLoadCenter { get; set; }
    byte TerrainLoadSideLength { get; set; }

    // Ent synchronization
    Vec2i? SyncCloc { get; set; }

    // Player
    ConcurrentQueue<MovePlayerCommand>? PendingMovement { get; set; }
    int PendingMovementWait { get; set; }
    bool IsSpawnReserved { get; set; }
}
