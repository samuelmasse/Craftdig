namespace Craftdig;

internal sealed class ServerPresenceIntake(
    ServerIdentitySessionEvents inbox,
    ServerPresenceMetrics metrics,
    ServerPresenceSessions sessions)
{
    private const double ProofInputsPerSecond = 150d;
    private const double ProofInputBurst = 15d;

    private double proofInputTokens = ProofInputBurst;
    private long lastProofTokenTimestamp;

    public void Initialize(long now) => lastProofTokenTimestamp = now;

    public void DrainChallenges()
    {
        while (inbox.TryTakeChallenge(out var input))
        {
            if (!sessions.TryGet(input.Connection, out var session) || input.Connection.IsCanceled)
            {
                metrics.RecordChallengeRejected();
                continue;
            }

            if (session.LatestChallenge != null && input.Challenge.Sequence <= session.LatestChallengeSequence)
            {
                metrics.RecordChallengeRejected();
                continue;
            }

            session.LatestChallengeSequence = input.Challenge.Sequence;
            session.LatestChallenge = input.Challenge;
            session.LatestChallengeTimestamp = input.ReceivedTimestamp;
            metrics.RecordChallengeAccepted();
        }
    }

    public void DrainProofs(long now, ServerPresenceRound? currentRound, ServerPresenceRound? previousRound)
    {
        RefillProofTokens(now);
        while (proofInputTokens >= 1d && inbox.TryTakeProof(out var input))
        {
            proofInputTokens -= 1d;
            ProcessProof(input, now, currentRound, previousRound);
        }
    }

    private void RefillProofTokens(long now)
    {
        long elapsed = now - lastProofTokenTimestamp;
        if (elapsed <= 0)
            return;

        proofInputTokens = Math.Min(
            ProofInputBurst,
            proofInputTokens + elapsed * ProofInputsPerSecond / Stopwatch.Frequency);
        lastProofTokenTimestamp = now;
    }

    private void ProcessProof(
        ServerPresenceProofInput input,
        long now,
        ServerPresenceRound? currentRound,
        ServerPresenceRound? previousRound)
    {
        if (!sessions.TryGet(input.Connection, out var session) || input.Connection.IsCanceled)
        {
            metrics.RecordProofRejected();
            return;
        }

        if (FindProofRound(input.Proof.RoundHash, now, currentRound, previousRound) is not { } round ||
            session.HasProofFor(round.Id) ||
            session.Identity is not { } identity ||
            input.Proof.TicketHash != identity.Ticket.TicketHash ||
            identity.Ticket.ExpiresAt <= DateTimeOffset.UtcNow ||
            !round.Contains(input.Connection.SessionId))
        {
            RejectUnusable(session);
            return;
        }

        session.RecordProofFor(round.Id);
        var digest = PresenceProofDigest.Compute(
            session.ServerContextHash,
            round.RoundHash,
            identity.Ticket.TicketHash);
        if (!session.VerifyProof(digest, input.Proof.Signature))
        {
            metrics.RecordProofRejected();
            sessions.Reject(input.Connection, "sent an invalid presence signature");
            return;
        }

        round.AddProof(new(identity.Ticket.TicketHash, input.Proof.Signature), now);
        session.RejectedProofInputs = 0;
        metrics.RecordProofAccepted();
    }

    private void RejectUnusable(ServerPresenceSession session)
    {
        metrics.RecordProofRejected();
        session.RejectedProofInputs++;

        if (session.RejectedProofInputs >= 8)
            sessions.Reject(session.Connection, "repeatedly submitted unusable presence proofs");
    }

    private static ServerPresenceRound? FindProofRound(
        Hash256 roundHash,
        long now,
        ServerPresenceRound? currentRound,
        ServerPresenceRound? previousRound)
    {
        if (currentRound is { } current && current.RoundHash == roundHash)
            return current;
        if (previousRound is { } previous && previous.RoundHash == roundHash &&
            now - previous.StartedTimestamp <= ServerPresenceTiming.DrainingRoundLifetime)
            return previous;
        return null;
    }
}
