namespace Crafthoe.Server;

[Components]
file record DimensionServerComponents(
    EntObj SocketPlayer,
    HashSet<Vector2i>? SocketStreamedChunks
);
