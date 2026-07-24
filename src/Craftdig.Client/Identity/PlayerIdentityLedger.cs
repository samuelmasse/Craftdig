namespace Craftdig.Client;

internal sealed class PlayerIdentityLedger
{
    private readonly Dictionary<Hash256, ValidatedIdentityTicket> ticketsByHash = [];
    private readonly Dictionary<Guid, Hash256> currentTicketByPlayerId = [];
    private readonly Dictionary<Guid, List<Hash256>> ticketHistoryByPlayerId = [];
    private readonly Dictionary<Guid, (SessionId SessionId, P256PublicKey PublicKey)> bindingByPlayerId = [];
    private readonly Dictionary<SessionId, (Guid PlayerId, P256PublicKey PublicKey)> bindingBySessionId = [];
    private readonly Dictionary<P256PublicKey, (Guid PlayerId, SessionId SessionId)> bindingByPublicKey = [];

    public int StoredPlayerCount => ticketHistoryByPlayerId.Count;
    public IReadOnlyCollection<Guid> PlayersWithTickets => currentTicketByPlayerId.Keys;

    public bool HasTicket(Hash256 ticketHash) => ticketsByHash.ContainsKey(ticketHash);

    public bool TryGetTicket(Hash256 ticketHash, [NotNullWhen(true)] out ValidatedIdentityTicket? ticket) =>
        ticketsByHash.TryGetValue(ticketHash, out ticket);

    public ValidatedIdentityTicket? CurrentTicket(Guid playerId) =>
        currentTicketByPlayerId.TryGetValue(playerId, out var currentHash)
            ? ticketsByHash.GetValueOrDefault(currentHash)
            : null;

    public PlayerIdentityLedgerChange Add(ValidatedIdentityTicket ticket, ReadOnlySpan<byte> rawTicket)
    {
        if (ticketsByHash.TryGetValue(ticket.TicketHash, out var existing))
        {
            return existing.RawTicket.SequenceEqual(rawTicket)
                ? PlayerIdentityLedgerChange.None
                : PlayerIdentityLedgerChange.Invalidating(existing.PlayerId, ticket.PlayerId);
        }

        if (!ticketHistoryByPlayerId.ContainsKey(ticket.PlayerId) &&
            ticketHistoryByPlayerId.Count >= ProtocolLimits.MaxPresencePlayers)
        {
            return PlayerIdentityLedgerChange.None;
        }

        var change = RecordBindings(ticket) with { Stored = true };
        ticketsByHash.Add(ticket.TicketHash, ticket);
        if (!ticketHistoryByPlayerId.TryGetValue(ticket.PlayerId, out var history))
        {
            history = [];
            ticketHistoryByPlayerId.Add(ticket.PlayerId, history);
        }
        history.Add(ticket.TicketHash);

        if (!currentTicketByPlayerId.TryGetValue(ticket.PlayerId, out var currentHash) ||
            ticket.IssuedAt >= ticketsByHash[currentHash].IssuedAt)
        {
            currentTicketByPlayerId[ticket.PlayerId] = ticket.TicketHash;
        }

        if (history.Count > 2)
        {
            var oldestHash = history.MinBy(hash => ticketsByHash[hash].IssuedAt);
            history.Remove(oldestHash);
            ticketsByHash.Remove(oldestHash);
            change = change with { EvictedTicket = oldestHash };
        }

        return change;
    }

    public HashSet<Hash256> Forget(Guid playerId)
    {
        currentTicketByPlayerId.Remove(playerId);
        var removedHashes = new HashSet<Hash256>();
        if (ticketHistoryByPlayerId.Remove(playerId, out var history))
        {
            foreach (var ticketHash in history)
            {
                removedHashes.Add(ticketHash);
                ticketsByHash.Remove(ticketHash);
            }
        }

        if (bindingByPlayerId.Remove(playerId, out var binding))
        {
            if (bindingBySessionId.TryGetValue(binding.SessionId, out var sessionBinding) &&
                sessionBinding.PlayerId == playerId)
                bindingBySessionId.Remove(binding.SessionId);
            if (bindingByPublicKey.TryGetValue(binding.PublicKey, out var keyBinding) &&
                keyBinding.PlayerId == playerId)
                bindingByPublicKey.Remove(binding.PublicKey);
        }

        return removedHashes;
    }

    public void Clear()
    {
        ticketsByHash.Clear();
        currentTicketByPlayerId.Clear();
        ticketHistoryByPlayerId.Clear();
        bindingByPlayerId.Clear();
        bindingBySessionId.Clear();
        bindingByPublicKey.Clear();
    }

    private PlayerIdentityLedgerChange RecordBindings(ValidatedIdentityTicket ticket)
    {
        List<Guid>? invalid = null;
        if (bindingByPlayerId.TryGetValue(ticket.PlayerId, out var playerBinding) &&
            (playerBinding.SessionId != ticket.SessionId || playerBinding.PublicKey != ticket.PublicKey))
            Invalidate(ref invalid, ticket.PlayerId);

        if (bindingBySessionId.TryGetValue(ticket.SessionId, out var sessionBinding) &&
            (sessionBinding.PlayerId != ticket.PlayerId || sessionBinding.PublicKey != ticket.PublicKey))
            Invalidate(ref invalid, ticket.PlayerId, sessionBinding.PlayerId);

        if (bindingByPublicKey.TryGetValue(ticket.PublicKey, out var keyBinding) &&
            (keyBinding.PlayerId != ticket.PlayerId || keyBinding.SessionId != ticket.SessionId))
            Invalidate(ref invalid, ticket.PlayerId, keyBinding.PlayerId);

        if (invalid == null)
        {
            bindingByPlayerId.TryAdd(ticket.PlayerId, (ticket.SessionId, ticket.PublicKey));
            bindingBySessionId.TryAdd(ticket.SessionId, (ticket.PlayerId, ticket.PublicKey));
            bindingByPublicKey.TryAdd(ticket.PublicKey, (ticket.PlayerId, ticket.SessionId));
        }

        return new(false, null, invalid ?? []);
    }

    private static void Invalidate(ref List<Guid>? invalid, params ReadOnlySpan<Guid> playerIds)
    {
        invalid ??= [];
        foreach (Guid playerId in playerIds)
        {
            if (!invalid.Contains(playerId))
                invalid.Add(playerId);
        }
    }
}
