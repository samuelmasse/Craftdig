namespace Craftdig.Server;

[Components]
public interface IServerComponents
{
    // Socket
    EntPtrIdx SocketWorldPlayer { get; set; }
    ClientThreadExecution SocketThread { get; set; }
    DateTime ConnectedTime { get; set; }
    string? AuthNonce { get; set; }
    string? AuthenticatedUid { get; set; }
    string? AuthenticatedUsername { get; set; }
    bool IsAuthenticated { get; set; }

    // Scratched
    bool IsScratched { get; set; }
    ulong[] ScratchedComponents { get; set; }
}
