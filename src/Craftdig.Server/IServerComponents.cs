namespace Craftdig.Server;

[Components]
public interface IServerComponents
{
    ClientThreadExecution SocketThread { get; set; }
    DateTime ConnectedTime { get; set; }
    string? AuthNonce { get; set; }
    string? AuthenticatedUid { get; set; }
    string? AuthenticatedUsername { get; set; }
    bool IsAuthenticated { get; set; }
}
