namespace Crafthoe.Server;

[Components]
file record ServerComponents(
    ClientThreadExecution SocketThread,
    DateTime ConnectedTime,
    string? AuthenticatedEmail,
    string? AuthenticatedUid,
    bool IsAuthenticated
);
