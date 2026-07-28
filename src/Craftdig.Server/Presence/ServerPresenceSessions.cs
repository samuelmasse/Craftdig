namespace Craftdig.Server;

internal sealed class ServerPresenceSessions(
    Log log,
    ServerPresenceMetrics metrics,
    int sessionCapacity,
    int identityQueueCapacity)
{
    private readonly List<ServerPresenceRegistration> pendingRegistrations = [];
    private readonly List<ServerPresenceSession> sessions = [];
    private readonly Dictionary<ServerPresenceConnection, ServerPresenceSession> byConnection = [];
    private readonly Dictionary<SessionId, ServerPresenceSession> bySessionId = [];

    public IReadOnlyList<ServerPresenceSession> All => sessions;

    public bool TryGet(ServerPresenceConnection connection, [NotNullWhen(true)] out ServerPresenceSession? session) =>
        byConnection.TryGetValue(connection, out session);

    public void DrainRegistrations(ServerIdentitySessionEvents inbox)
    {
        while (inbox.TryTakeRegistration(out var registration))
            pendingRegistrations.Add(registration);
    }

    public void ApplyActivated()
    {
        for (int i = 0; i < pendingRegistrations.Count;)
        {
            var registration = pendingRegistrations[i];
            if (!registration.Connection.IsCancelled && !registration.IsActivated)
            {
                i++;
                continue;
            }

            pendingRegistrations.RemoveAt(i);
            if (!registration.Connection.IsCancelled)
                Apply(registration);
        }
    }

    public void RemoveDisconnected(ServerIdentitySessionEvents inbox)
    {
        while (inbox.TryTakeDisconnection(out var connection))
            Remove(connection);

        for (int i = sessions.Count - 1; i >= 0; i--)
        {
            if (sessions[i].Connection.IsCancelled)
                Remove(sessions[i].Connection);
        }
    }

    public void Reject(ServerPresenceConnection connection, string reason)
    {
        if (connection.IsCancelled)
            return;

        log.Warn("Socket {0} presence session {1} {2}", connection.Socket.Tag, connection.SessionId, reason);
        Close(connection);
    }

    public void Shutdown(ServerIdentitySessionEvents inbox)
    {
        inbox.StopAccepting();
        while (inbox.TryTakeRegistration(out var registration))
            pendingRegistrations.Add(registration);

        foreach (var registration in pendingRegistrations)
            Close(registration.Connection);
        pendingRegistrations.Clear();

        foreach (var session in sessions)
        {
            Close(session.Connection);
            session.Dispose();
        }
        sessions.Clear();
        byConnection.Clear();
        bySessionId.Clear();
        metrics.SetActiveSessions(0);
    }

    private void Apply(ServerPresenceRegistration registration)
    {
        if (registration.Refresh)
            ApplyRefresh(registration);
        else
            ApplyJoin(registration);
    }

    private void ApplyRefresh(ServerPresenceRegistration registration)
    {
        var connection = registration.Connection;
        if (!byConnection.TryGetValue(connection, out var refreshed) || registration.Identity == null)
        {
            Reject(connection, "sent a presence refresh for an unknown connection");
            return;
        }

        refreshed.InstallIdentity(registration.Identity);
        foreach (var session in sessions)
            QueueIdentity(session, registration.Identity.Ticket);
    }

    private void ApplyJoin(ServerPresenceRegistration registration)
    {
        var connection = registration.Connection;
        if (sessions.Count >= sessionCapacity ||
            byConnection.ContainsKey(connection) ||
            bySessionId.ContainsKey(connection.SessionId))
        {
            Reject(connection, "reused a presence session ID");
            return;
        }

        var added = new ServerPresenceSession(connection, identityQueueCapacity);
        if (registration.Identity is { } identity)
            added.InstallIdentity(identity);
        if (!TrySeedIdentitySnapshot(added, registration.Identity))
            return;

        sessions.Add(added);
        byConnection.Add(connection, added);
        bySessionId.Add(connection.SessionId, added);
        metrics.SetActiveSessions(sessions.Count);

        if (registration.Identity is { } joinedIdentity)
        {
            foreach (var viewer in sessions)
            {
                if (!ReferenceEquals(viewer, added))
                    QueueIdentity(viewer, joinedIdentity.Ticket);
            }
        }
    }

    private bool TrySeedIdentitySnapshot(ServerPresenceSession added, ServerIdentitySessionSnapshot? ownIdentity)
    {
        foreach (var existing in sessions)
        {
            if (existing.Identity is { } existingIdentity && !added.TryEnqueueIdentity(existingIdentity.Ticket))
            {
                added.Dispose();
                Reject(added.Connection, "could not queue its initial Identity snapshot");
                return false;
            }
        }

        if (ownIdentity != null && !added.TryEnqueueIdentity(ownIdentity.Ticket))
        {
            added.Dispose();
            Reject(added.Connection, "could not queue its own Identity ticket");
            return false;
        }

        if (added.QueuedIdentityCount == 0)
            added.InitialIdentitySnapshotPending = false;

        return true;
    }

    private void QueueIdentity(ServerPresenceSession session, ValidatedIdentityTicket ticket)
    {
        if (session.TryEnqueueIdentity(ticket))
            return;

        metrics.RecordSlowSocketDisconnect();
        Reject(session.Connection, "fell behind the bounded Identity output queue");
    }

    private void Remove(ServerPresenceConnection connection)
    {
        if (!byConnection.Remove(connection, out var session))
            return;

        if (bySessionId.TryGetValue(connection.SessionId, out var indexed) && ReferenceEquals(indexed, session))
            bySessionId.Remove(connection.SessionId);
        sessions.Remove(session);
        session.Dispose();
        metrics.SetActiveSessions(sessions.Count);
    }

    private static void Close(ServerPresenceConnection connection)
    {
        if (connection.IsCancelled)
            return;

        connection.Cancel();
        connection.Socket.Disconnect();
    }
}
