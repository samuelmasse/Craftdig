namespace Craftdig;

internal sealed class PlayerPresenceRounds(PlayerIdentityCache identityCache)
{
    private const int MaxPendingProofs = ProtocolLimits.MaxPresencePlayers * ProtocolLimits.MaxPresenceActiveRounds;

    private readonly List<PlayerPresenceRound> rounds = [];
    private readonly List<(Hash256 RoundHash, PresenceProofRecord Proof)> pendingProofs = [];
    private readonly HashSet<(Hash256 RoundHash, Hash256 TicketHash)> receivedProofs = [];
    private readonly HashSet<(Hash256 RoundHash, Hash256 TicketHash)> duplicateProofs = [];
    private readonly Dictionary<(Hash256 RoundHash, Guid PlayerId), Hash256> proofTicketByPlayer = [];
    private long roundOrder;

    public IReadOnlyList<PlayerPresenceRound> All => rounds;

    public PlayerPresenceRound? Find(Hash256 roundHash)
    {
        foreach (var round in rounds)
        {
            if (round.RoundHash == roundHash)
                return round;
        }

        return null;
    }

    public void AcceptChunk(
        PresenceRoundChunkHeader header,
        ReadOnlySpan<byte> recordBytes,
        ReadOnlySpan<(ulong Sequence, Nonce256 Nonce, long Deadline)> pendingChallenges,
        long now)
    {
        var round = Find(header.RoundHash);
        if (round == null)
        {
            if (rounds.Count == ProtocolLimits.MaxPresenceActiveRounds)
                Evict(OldestRound());

            round = new(header, ++roundOrder);
            rounds.Add(round);
        }

        round.AddChunk(header, recordBytes);
        if (!round.Complete && !round.Invalid)
            round.CompleteRound(pendingChallenges, now);
    }

    public void RegisterProofRecord(
        Hash256 roundHash,
        PresenceProofRecord proof,
        PlayerIdentityLedger ledger,
        PlayerPresenceRoster roster)
    {
        var key = (roundHash, proof.TicketHash);
        if (receivedProofs.Count >= MaxPendingProofs && !receivedProofs.Contains(key))
        {
            return;
        }

        if (!receivedProofs.Add(key))
        {
            duplicateProofs.Add(key);
            if (ledger.TryGetTicket(proof.TicketHash, out var duplicateTicket))
                roster.MarkInvalid(duplicateTicket.PlayerId);
            return;
        }

        if (pendingProofs.Count < MaxPendingProofs)
            pendingProofs.Add((roundHash, proof));
    }

    public bool TryTakeProof(
        long now,
        PlayerIdentityLedger ledger,
        PlayerPresenceRoster roster,
        out Hash256 roundHash,
        out PresenceProofRecord proof)
    {
        DropExpiredProofs(now, ledger, roster);

        int backgroundReady = -1;
        int playerListReady = -1;
        int selected = -1;
        bool playerListOpen = identityCache.IsPlayerListOpen;
        for (int i = 0; i < pendingProofs.Count && selected < 0; i++)
        {
            var (RoundHash, Proof) = pendingProofs[i];
            var round = Find(RoundHash)!;
            if (!round.Complete || !round.Usable || !ledger.TryGetTicket(Proof.TicketHash, out var ticket))
                continue;

            if (identityCache.IsEntPresent(ticket.PlayerId))
                selected = i;
            else if (playerListOpen && playerListReady < 0)
                playerListReady = i;
            else if (backgroundReady < 0)
                backgroundReady = i;
        }

        if (selected < 0)
            selected = playerListReady >= 0 ? playerListReady : backgroundReady;
        if (selected < 0)
        {
            roundHash = default;
            proof = default;
            return false;
        }

        (roundHash, proof) = pendingProofs[selected];
        pendingProofs.RemoveAt(selected);
        return true;
    }

    public bool HasDuplicateProof(Hash256 roundHash, Hash256 ticketHash) =>
        duplicateProofs.Contains((roundHash, ticketHash));

    public bool HasDuplicateProofForTicket(Hash256 ticketHash)
    {
        foreach (var (_, duplicateTicketHash) in duplicateProofs)
        {
            if (duplicateTicketHash == ticketHash)
                return true;
        }

        return false;
    }

    public bool RecordProofTicket(Hash256 roundHash, Guid playerId, Hash256 ticketHash)
    {
        var key = (roundHash, playerId);
        if (proofTicketByPlayer.TryGetValue(key, out var priorTicketHash) && priorTicketHash != ticketHash)
            return false;

        proofTicketByPlayer[key] = ticketHash;
        return true;
    }

    public void DropPendingProofs(Hash256 ticketHash) =>
        pendingProofs.RemoveAll(pending => pending.Proof.TicketHash == ticketHash);

    public void DropTicketProofs(HashSet<Hash256> ticketHashes)
    {
        pendingProofs.RemoveAll(pending => ticketHashes.Contains(pending.Proof.TicketHash));
        receivedProofs.RemoveWhere(key => ticketHashes.Contains(key.TicketHash));
        duplicateProofs.RemoveWhere(key => ticketHashes.Contains(key.TicketHash));
    }

    public void ForgetPlayer(Guid playerId)
    {
        foreach (var key in proofTicketByPlayer.Keys.Where(key => key.PlayerId == playerId).ToArray())
            proofTicketByPlayer.Remove(key);
    }

    public void Clear()
    {
        rounds.Clear();
        pendingProofs.Clear();
        receivedProofs.Clear();
        duplicateProofs.Clear();
        proofTicketByPlayer.Clear();
    }

    private void DropExpiredProofs(long now, PlayerIdentityLedger ledger, PlayerPresenceRoster roster)
    {
        for (int i = pendingProofs.Count - 1; i >= 0; i--)
        {
            var (RoundHash, Proof) = pendingProofs[i];
            var round = Find(RoundHash);
            if (round == null || round.Invalid || (round.Complete && now > round.LocalDeadline))
            {
                if (ledger.TryGetTicket(Proof.TicketHash, out var staleTicket))
                    roster.MarkProofSeen(staleTicket.PlayerId);
                pendingProofs.RemoveAt(i);
            }
        }
    }

    private void Evict(PlayerPresenceRound round)
    {
        rounds.Remove(round);
        pendingProofs.RemoveAll(pending => pending.RoundHash == round.RoundHash);
        receivedProofs.RemoveWhere(key => key.RoundHash == round.RoundHash);
        duplicateProofs.RemoveWhere(key => key.RoundHash == round.RoundHash);
        foreach (var key in proofTicketByPlayer.Keys.Where(key => key.RoundHash == round.RoundHash).ToArray())
            proofTicketByPlayer.Remove(key);
    }

    private PlayerPresenceRound OldestRound()
    {
        var oldest = rounds[0];
        for (int i = 1; i < rounds.Count; i++)
        {
            if (rounds[i].Order < oldest.Order)
                oldest = rounds[i];
        }

        return oldest;
    }
}
