namespace Crafthoe.Server;

[Components]
file record ServerComponents(
    EntObj SocketPlayer,
    HashSet<Vector2i>? SocketStreamedChunks
);
