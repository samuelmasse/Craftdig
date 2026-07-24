namespace Craftdig.Client;

internal sealed class PlayerProofVerifier(
    PlayerIdentitySession identitySession,
    PlayerIdentityLedger ledger,
    PlayerPresenceRounds rounds,
    PlayerPresenceRoster roster,
    PlayerSocket socket)
{
    private const int MaxRecentSignedRounds = ProtocolLimits.MaxPresenceActiveRounds * 2;

    private readonly PlayerPresenceSigningGuard signingGuard = new(MaxRecentSignedRounds);
    private readonly Hash256? serverContextHash = identitySession.ServerContext?.ComputeHash();

    public bool VerifyNext()
    {
        if (serverContextHash == null ||
            !rounds.TryTakeProof(Stopwatch.GetTimestamp(), ledger, roster, out var roundHash, out var proof))
            return false;

        var round = rounds.Find(roundHash)!;
        ledger.TryGetTicket(proof.TicketHash, out var ticket);
        roster.MarkProofSeen(ticket!.PlayerId);

        if (rounds.HasDuplicateProof(roundHash, proof.TicketHash) || roster.IsInvalid(ticket.PlayerId))
        {
            roster.MarkInvalid(ticket.PlayerId);
            return true;
        }

        if (!rounds.RecordProofTicket(roundHash, ticket.PlayerId, proof.TicketHash))
        {
            roster.MarkInvalid(ticket.PlayerId);
            return true;
        }

        long now = Stopwatch.GetTimestamp();
        if (!round.Usable || now > round.LocalDeadline || ticket.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return true;
        }

        if (!round.ContainsSession(ticket.SessionId))
        {
            roster.MarkInvalid(ticket.PlayerId);
            return true;
        }

        if (!HasValidSignature(ticket, round, proof))
        {
            roster.MarkInvalid(ticket.PlayerId);
            return true;
        }

        roster.SetVerified(ticket, round.RoundHash, round.LocalDeadline);
        roster.Touch(ticket.PlayerId, now);
        return true;
    }

    private bool HasValidSignature(ValidatedIdentityTicket ticket, PlayerPresenceRound round, PresenceProofRecord proof)
    {
        var digest = PresenceProofDigest.Compute(serverContextHash!.Value, round.RoundHash, ticket.TicketHash);
        using var publicKey = ticket.PublicKey.CreateEcdsa();
        try
        {
            return proof.Signature.VerifyHash(publicKey, digest);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    public void SignRounds()
    {
        if (!identitySession.IdentityEnabled)
            return;

        long now = Stopwatch.GetTimestamp();
        signingGuard.Expire(now);

        Span<byte> body = stackalloc byte[PresenceProofCommandCodec.Size];
        foreach (var round in rounds.All)
        {
            if (signingGuard.Contains(round.RoundHash))
            {
                round.Signed = true;
                continue;
            }

            if (round.Invalid || !round.Complete || !round.Usable || round.Signed ||
                round.OwnRoundSessionId != identitySession.SessionId)
                continue;

            if (now > round.LocalDeadline)
                continue;
            if (!signingGuard.HasCapacity)
                continue;

            var ownTicket = identitySession.GetCurrentTicket();
            if (ownTicket == null)
                continue;
            if (!ledger.HasTicket(ownTicket.TicketHash))
                continue;
            if (!identitySession.TrySignPresence(round.RoundHash, ownTicket.TicketHash, out var signature))
                continue;

            if (!signingGuard.TryRecord(round.RoundHash, round.LocalDeadline))
                continue;

            var proof = new PresenceProof(round.RoundHash, ownTicket.TicketHash, signature);
            PresenceProofCommandCodec.TryWrite(body, proof, out int written);
            socket.SendRaw<PresenceProofCommand>(body[..written]);
            round.Signed = true;
        }
    }

    public void Clear() => signingGuard.Clear();
}
