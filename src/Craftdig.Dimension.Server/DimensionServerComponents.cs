namespace Craftdig.Dimension.Server;

[Components]
file record DimensionServerComponents(
    // Socket
    EntObj SocketPlayer,
    HashSet<Vector2i>? SocketStreamedChunks,
    HashSet<Vector3i>? SocketForgottenSections,
    Queue<Vector3i>? SocketForgottenSectionQueue,

    // Player
    ConcurrentQueue<MovePlayerCommand>? PendingMovement,
    int PendingMovementWait,
    int PendingMovementLongWait
);
