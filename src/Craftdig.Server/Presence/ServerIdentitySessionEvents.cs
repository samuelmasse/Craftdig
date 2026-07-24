namespace Craftdig.Server;

[Server]
public class ServerIdentitySessionEvents(ServerConfig config)
{
    private readonly Lock gate = new();
    private readonly AutoResetEvent pulse = new(false);
    private readonly Queue<ServerPresenceRegistration> registrations = [];
    private readonly Queue<ServerPresenceConnection> disconnections = [];
    private readonly Dictionary<ServerPresenceConnection, ServerPresenceChallengeInput> challenges = [];
    private readonly Dictionary<(ServerPresenceConnection Connection, Hash256 RoundHash), ServerPresenceProofInput> proofs = [];
    private readonly Dictionary<ServerPresenceConnection, int> proofCounts = [];
    private readonly HashSet<ServerPresenceConnection> reservedConnections = [];
    private readonly HashSet<SessionId> reservedSessionIds = [];
    private readonly int sessionCapacity = ValidateSessionCapacity(config.MaxPlayers);
    private bool accepting = true;

    public bool TryPublishIdentity(
        NetSocket socket,
        ServerIdentitySessionSnapshot identity,
        bool refresh,
        out ServerPresenceRegistration registration)
    {
        registration = null!;
        ServerPresenceConnection connection;
        if (refresh)
        {
            connection = socket.PresenceConnection!;
            if (connection == null || connection.IsCancelled ||
                connection.Generation != socket.ConnectionGeneration ||
                connection.SessionId != identity.Ticket.SessionId)
                return false;
        }
        else
        {
            connection = new(socket, socket.ConnectionGeneration, identity.Ticket.SessionId);
        }

        var candidate = new ServerPresenceRegistration(connection, identity, refresh);
        lock (gate)
        {
            if (!accepting || registrations.Count >= sessionCapacity * 2 ||
                refresh && !reservedConnections.Contains(connection) ||
                !refresh && !TryReserve(connection))
                return false;

            registrations.Enqueue(candidate);
        }

        registration = candidate;
        pulse.Set();
        return true;
    }

    public bool TryPublishUnverified(
        NetSocket socket,
        out ServerPresenceRegistration registration)
    {
        lock (gate)
        {
            if (!accepting || registrations.Count >= sessionCapacity * 2)
            {
                registration = null!;
                return false;
            }

            ServerPresenceConnection connection;
            do
            {
                connection = new(
                    socket,
                    socket.ConnectionGeneration,
                    SessionId.CreateRandom());
            }
            while (reservedSessionIds.Contains(connection.SessionId));

            if (!TryReserve(connection))
            {
                registration = null!;
                return false;
            }

            registration = new(connection, null, false);
            registrations.Enqueue(registration);
        }

        pulse.Set();
        return true;
    }

    public void PublishDisconnected(NetSocket socket)
    {
        if (socket.PresenceConnection is not { } connection)
            return;

        connection.Cancel();
        lock (gate)
        {
            if (reservedConnections.Remove(connection))
                reservedSessionIds.Remove(connection.SessionId);

            if (disconnections.Count < sessionCapacity * 2)
                disconnections.Enqueue(connection);
        }

        pulse.Set();
    }

    public bool TryPublishChallenge(ServerPresenceConnection connection, PresenceChallenge challenge)
    {
        lock (gate)
        {
            if (!accepting || connection.IsCancelled)
                return false;

            if (challenges.TryGetValue(connection, out var pending))
            {
                if (challenge.Sequence <= pending.Challenge.Sequence)
                    return true;
            }
            else if (challenges.Count >= sessionCapacity)
            {
                return false;
            }

            challenges[connection] = new(connection, challenge, Stopwatch.GetTimestamp());
        }

        pulse.Set();
        return true;
    }

    public bool TryPublishProof(ServerPresenceConnection connection, PresenceProof proof)
    {
        lock (gate)
        {
            if (!accepting || connection.IsCancelled)
                return false;

            var key = (connection, proof.RoundHash);
            if (proofs.ContainsKey(key))
                return true;
            if (proofs.Count >= sessionCapacity * ProtocolLimits.MaxPresenceActiveRounds ||
                proofCounts.GetValueOrDefault(connection) >= ProtocolLimits.MaxPresenceActiveRounds)
                return false;

            proofs.Add(key, new(connection, proof));
            proofCounts[connection] = proofCounts.GetValueOrDefault(connection) + 1;
        }

        pulse.Set();
        return true;
    }

    public bool TryTakeRegistration([MaybeNullWhen(false)] out ServerPresenceRegistration registration)
    {
        lock (gate)
        {
            if (registrations.Count == 0)
            {
                registration = null;
                return false;
            }

            registration = registrations.Dequeue();
            return true;
        }
    }

    public bool TryTakeDisconnection([MaybeNullWhen(false)] out ServerPresenceConnection connection)
    {
        lock (gate)
        {
            if (disconnections.Count == 0)
            {
                connection = null;
                return false;
            }

            connection = disconnections.Dequeue();
            return true;
        }
    }

    public bool TryTakeChallenge(out ServerPresenceChallengeInput input)
    {
        lock (gate)
        {
            var enumerator = challenges.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                enumerator.Dispose();
                input = item.Value;
                challenges.Remove(item.Key);
                return true;
            }

            enumerator.Dispose();
            input = default;
            return false;
        }
    }

    public bool TryTakeProof(out ServerPresenceProofInput input)
    {
        lock (gate)
        {
            var enumerator = proofs.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                enumerator.Dispose();
                input = item.Value;
                proofs.Remove(item.Key);
                DecrementProofCount(item.Value.Connection);
                return true;
            }

            enumerator.Dispose();
            input = default;
            return false;
        }
    }

    public void Signal() => pulse.Set();

    public void Wait(TimeSpan timeout) => pulse.WaitOne(timeout);

    public ServerPresenceInboxDepths Depths()
    {
        lock (gate)
            return new(registrations.Count + disconnections.Count, challenges.Count, proofs.Count);
    }

    public void StopAccepting()
    {
        lock (gate)
            accepting = false;
        pulse.Set();
    }

    private bool TryReserve(ServerPresenceConnection connection)
    {
        if (reservedConnections.Count >= sessionCapacity ||
            !reservedSessionIds.Add(connection.SessionId))
            return false;

        reservedConnections.Add(connection);
        return true;
    }

    private void DecrementProofCount(ServerPresenceConnection connection)
    {
        int remaining = proofCounts[connection] - 1;
        if (remaining == 0)
            proofCounts.Remove(connection);
        else
            proofCounts[connection] = remaining;
    }

    private static int ValidateSessionCapacity(int configured)
    {
        if (configured is < 1 or > ProtocolLimits.MaxPresencePlayers)
            throw new InvalidDataException(
                $"MaxPlayers must be between 1 and {ProtocolLimits.MaxPresencePlayers}.");

        return configured;
    }
}
