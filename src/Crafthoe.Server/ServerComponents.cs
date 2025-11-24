namespace Crafthoe.Server;

[Components]
file record ServerComponents(
    Thread? SocketThread,
    string? AuthenticatedEmail,
    string? AuthenticatedUid,
    bool IsAuthenticated
);
