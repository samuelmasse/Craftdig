namespace Craftdig.Dimension.Server;

[Components]
public interface IDimensionServerComponents
{
    // Socket
    EntPtrIdx SocketPlayer { get; set; }
    HashSet<Vector2i>? SocketStreamedChunks { get; set; }
    HashSet<Vector3i>? SocketForgottenSections { get; set; }
    Queue<Vector3i>? SocketForgottenSectionQueue { get; set; }

    // Player
    ConcurrentQueue<MovePlayerCommand>? PendingMovement { get; set; }
    int PendingMovementWait { get; set; }
}
