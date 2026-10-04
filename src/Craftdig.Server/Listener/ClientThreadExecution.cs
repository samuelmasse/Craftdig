namespace Craftdig;

public readonly record struct ClientThreadExecution(ClientThread ClientThread, long ExecutionId)
{
    public bool IsCompleted => ClientThread.CurrentExecutionId != ExecutionId;
}
