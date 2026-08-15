namespace Craftdig;

[Server]
public class ServerPresenceReceiver(
    Log log,
    ServerIdentitySessionEvents inbox,
    ServerPresenceMetrics metrics)
{
    public void Challenge(NetSocket socket, ReadOnlySpan<byte> body)
    {
        if (!TryGetConnection(socket, out var connection))
        {
            Reject(socket, "sent a presence challenge without an active authenticated presence connection", metrics.RecordChallengeRejected);
            return;
        }

        if (!PresenceChallengeCommandCodec.TryRead(body, out var challenge))
        {
            Reject(socket, "sent a malformed presence challenge", metrics.RecordChallengeRejected);
            return;
        }

        if (!inbox.TryPublishChallenge(connection, challenge))
            Reject(socket, "overflowed the presence challenge inbox", metrics.RecordChallengeRejected);
    }

    public void Proof(NetSocket socket, ReadOnlySpan<byte> body)
    {
        if (!TryGetConnection(socket, out var connection))
        {
            Reject(socket, "sent a presence proof without an active authenticated presence connection", metrics.RecordProofRejected);
            return;
        }

        if (socket.IdentitySession == null)
        {
            Reject(socket, "sent a presence proof from an unverified no-auth session", metrics.RecordProofRejected);
            return;
        }

        if (!PresenceProofCommandCodec.TryRead(body, out var proof))
        {
            Reject(socket, "sent a malformed presence proof", metrics.RecordProofRejected);
            return;
        }

        if (!inbox.TryPublishProof(connection, proof))
            Reject(socket, "overflowed the presence proof inbox", metrics.RecordProofRejected);
    }

    private bool TryGetConnection(
        NetSocket socket,
        [NotNullWhen(true)] out ServerPresenceConnection? connection)
    {
        connection = socket.PresenceConnection;
        return socket.IsAuthenticated && connection is { IsCancelled: false } &&
            connection.Generation == socket.ConnectionGeneration;
    }

    private void Reject(NetSocket socket, string reason, Action recordRejection)
    {
        recordRejection();
        log.Warn("Socket {0} {1}", socket.Tag, reason);
        socket.Disconnect();
    }
}
