namespace Craftdig.Dimension.Server;

[Components]
public interface IDimensionServerComponents
{
    // Socket
    EntPtrIdx SocketPlayer { get; set; }
    HashSet<Vec2i>? SocketStreamedChunks { get; set; }
    HashSet<Vec3i>? SocketForgottenSections { get; set; }
    Queue<Vec3i>? SocketForgottenSectionQueue { get; set; }

    // Player
    ConcurrentQueue<MovePlayerCommand>? PendingMovement { get; set; }
    int PendingMovementWait { get; set; }
}
