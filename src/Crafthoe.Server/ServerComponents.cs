namespace Crafthoe.Server;

[Components]
file record ServerComponents(
    Thread? SocketThread,
    string? AuthenticatedUid,
    bool IsAuthenticated
);
