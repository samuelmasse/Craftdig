namespace Craftdig;

public readonly record struct ServerSocketConnection(
    NetSocket Socket,
    ClientThreadExecution Receive,
    ClientThreadExecution Send)
{
    public bool IsCompleted => Receive.IsCompleted && Send.IsCompleted;
}
