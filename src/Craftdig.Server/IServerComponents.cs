namespace Craftdig.Server;

[Components]
public interface IServerComponents
{
    // Socket
    ClientThreadExecution SocketThread { get; set; }
    DateTime ConnectedTime { get; set; }
    long ConnectionGeneration { get; set; }
    ServerAuthChallenge? AuthChallenge { get; set; }
    ServerIdentitySessionSnapshot? IdentitySession { get; set; }
    ServerPresenceConnection? PresenceConnection { get; set; }
    string? AuthenticatedUid { get; set; }
    string? AuthenticatedUsername { get; set; }
    bool IsAuthenticated { get; set; }
}
