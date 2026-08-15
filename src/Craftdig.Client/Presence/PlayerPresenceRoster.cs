namespace Craftdig;

internal sealed class PlayerPresenceRoster(PlayerIdentitySession identitySession, PlayerIdentityCache identityCache)
{
    private static readonly long RetentionTicks = MonotonicTime.DurationTicks(TimeSpan.FromSeconds(30));
    private static readonly long PublishIntervalTicks = MonotonicTime.DurationTicks(TimeSpan.FromSeconds(1));

    private readonly HashSet<Guid> invalidPlayers = [];
    private readonly HashSet<Guid> proofSeenPlayers = [];
    private readonly Dictionary<Guid, (
        ValidatedIdentityTicket Ticket,
        Hash256 RoundHash,
        long Deadline)> verifiedPlayers = [];
    private readonly Dictionary<Guid, long> lastIdentityActivity = [];
    private long nextPublishAt;

    public void MarkInvalid(Guid playerId) => invalidPlayers.Add(playerId);

    public bool IsInvalid(Guid playerId) => invalidPlayers.Contains(playerId);

    public void MarkProofSeen(Guid playerId) => proofSeenPlayers.Add(playerId);

    public void Touch(Guid playerId, long monotonicNow) => lastIdentityActivity[playerId] = monotonicNow;

    public void SetVerified(ValidatedIdentityTicket ticket, Hash256 roundHash, long deadline) =>
        verifiedPlayers[ticket.PlayerId] = (ticket, roundHash, deadline);

    public void PublishIfDue(long monotonicNow, PlayerIdentityLedger ledger, PlayerPresenceRounds rounds)
    {
        if (monotonicNow < nextPublishAt)
            return;

        nextPublishAt = monotonicNow + PublishIntervalTicks;
        PruneInactive(monotonicNow, ledger, rounds);
        identityCache.Publish(BuildSnapshots(monotonicNow, ledger, rounds));
    }

    public void PruneInactive(long monotonicNow, PlayerIdentityLedger ledger, PlayerPresenceRounds rounds)
    {
        foreach (var entry in lastIdentityActivity.ToArray())
        {
            if (identityCache.IsEntPresent(entry.Key) ||
                ShouldRetainRosterEntry(false, entry.Value, monotonicNow))
                continue;

            rounds.DropTicketProofs(ledger.Forget(entry.Key));
            rounds.ForgetPlayer(entry.Key);
            invalidPlayers.Remove(entry.Key);
            proofSeenPlayers.Remove(entry.Key);
            verifiedPlayers.Remove(entry.Key);
            lastIdentityActivity.Remove(entry.Key);
        }
    }

    public void Clear()
    {
        invalidPlayers.Clear();
        proofSeenPlayers.Clear();
        verifiedPlayers.Clear();
        lastIdentityActivity.Clear();
        nextPublishAt = 0;
        identityCache.Publish(new Dictionary<Guid, PlayerIdentitySnapshot>());
    }

    internal static bool ShouldRetainRosterEntry(bool entPresent, long? lastActivity, long monotonicNow) =>
        entPresent || lastActivity is { } activity && monotonicNow - activity <= RetentionTicks;

    private Dictionary<Guid, PlayerIdentitySnapshot> BuildSnapshots(
        long monotonicNow,
        PlayerIdentityLedger ledger,
        PlayerPresenceRounds rounds)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var published = new Dictionary<Guid, PlayerIdentitySnapshot>();
        foreach (Guid playerId in CollectRosterIds(ledger))
        {
            if (Describe(playerId, monotonicNow, utcNow, ledger, rounds) is { } snapshot)
                published[playerId] = snapshot;
        }

        return published;
    }

    private HashSet<Guid> CollectRosterIds(PlayerIdentityLedger ledger)
    {
        var ids = new HashSet<Guid>(ledger.PlayersWithTickets);
        ids.UnionWith(identityCache.CaptureEnts());
        ids.UnionWith(invalidPlayers);
        ids.UnionWith(verifiedPlayers.Keys);
        return ids;
    }

    private PlayerIdentitySnapshot? Describe(
        Guid playerId,
        long monotonicNow,
        DateTimeOffset utcNow,
        PlayerIdentityLedger ledger,
        PlayerPresenceRounds rounds)
    {
        bool entPresent = identityCache.IsEntPresent(playerId);
        long? lastActivity = lastIdentityActivity.TryGetValue(playerId, out long activity)
            ? activity
            : null;
        if (!ShouldRetainRosterEntry(entPresent, lastActivity, monotonicNow))
            return null;

        var currentTicket = ledger.CurrentTicket(playerId);
        verifiedPlayers.TryGetValue(playerId, out var verified);
        var verifiedTicket = verified.Ticket;
        var verifiedRound = verifiedTicket != null ? rounds.Find(verified.RoundHash) : null;
        var status = PlayerIdentityStatusPolicy.Evaluate(new()
        {
            HasVerifiableTransport = identitySession.ServerContext != null,
            IdentityExpected = identitySession.IdentityEnabled,
            EntPresent = entPresent,
            Invalid = invalidPlayers.Contains(playerId) || verifiedRound?.Invalid == true,
            ProofSeen = proofSeenPlayers.Contains(playerId),
            VerifiedRoundUsable = verifiedRound?.Usable == true,
            MonotonicNow = monotonicNow,
            UtcNow = utcNow,
            VerifiedDeadline = verifiedTicket != null ? verified.Deadline : null,
            VerifiedTicketExpiresAt = verifiedTicket?.ExpiresAt,
            CurrentTicketExpiresAt = currentTicket?.ExpiresAt,
        });

        var displayTicket = status == PlayerIdentityStatus.Verified ? verifiedTicket : currentTicket;
        return new(playerId, displayTicket?.Username, status, entPresent);
    }
}
