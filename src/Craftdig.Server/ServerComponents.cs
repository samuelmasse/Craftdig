namespace Craftdig.Server;

[Components]
file record ServerComponents(
    ClientThreadExecution SocketThread,
    DateTime ConnectedTime,
    string? AuthNonce,
    string? AuthenticatedUid,
    string? AuthenticatedUsername,
    bool IsAuthenticated
);
