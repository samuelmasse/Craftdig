namespace Craftdig.Server;

[Components]
file record ServerComponents(
    ClientThreadExecution SocketThread,
    DateTime ConnectedTime,
    string? AuthNonce,
    string? AuthenticatedEmail,
    string? AuthenticatedUid,
    bool IsAuthenticated
);
