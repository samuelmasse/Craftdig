namespace Crafthoe.Dimension.Server;

[Components]
file record DimensionServerComponents(
    // Socket
    EntObj SocketPlayer,
    HashSet<Vector2i>? SocketStreamedChunks,

    // Player
    ConcurrentQueue<MovementStep>? PendingMovement,
    int PendingMovementWait,
    int PendingMovementLongWait
);
