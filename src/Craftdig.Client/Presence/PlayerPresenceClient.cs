namespace Craftdig;

[Player]
public class PlayerPresenceClient
{
    private const int MaxInboundMessages = ProtocolLimits.MaxPresencePlayers * 4;
    private const int MaxInboundBytes = 8 * 1024 * 1024;
    private static readonly TimeSpan WorkerPollInterval = TimeSpan.FromMilliseconds(100);

    private readonly Log log;
    private readonly ModuleIdentityTrust identityTrust;
    private readonly PlayerIdentitySession identitySession;
    private readonly PlayerNetLoop loop;
    private readonly PlayerSocket socket;
    private readonly PlayerPresenceInbox inbox = new(MaxInboundMessages, MaxInboundBytes);
    private readonly PlayerIdentityLedger ledger = new();
    private readonly PlayerPresenceRounds rounds;
    private readonly PlayerPresenceRoster roster;
    private readonly PlayerChallengeScheduler challenges;
    private readonly PlayerProofVerifier verifier;

    private CancellationTokenSource? cancellation;
    private Thread? workerThread;
    private Thread? challengeThread;

    public PlayerPresenceClient(
        Log log,
        ModuleIdentityTrust identityTrust,
        PlayerIdentitySession identitySession,
        PlayerIdentityCache identityCache,
        PlayerNetLoop loop,
        PlayerSocket socket)
    {
        this.log = log;
        this.identityTrust = identityTrust;
        this.identitySession = identitySession;
        this.loop = loop;
        this.socket = socket;
        rounds = new(identityCache);
        roster = new(identitySession, identityCache);
        challenges = new(log, identitySession, inbox, socket);
        verifier = new(identitySession, ledger, rounds, roster, socket);
    }

    public void Start()
    {
        if (workerThread != null || challengeThread != null)
            throw new InvalidOperationException("The presence client is already running.");

        log.Info(
            "Starting client presence security; Identity enabled: {0}, server context available: {1}",
            identitySession.IdentityEnabled, identitySession.ServerContext != null);
        loop.RegisterGuarded<PlayerIdentityCommand>(
            static data => PlayerIdentityCommandCodec.TryRead(data, out _),
            (source, data) => inbox.Enqueue(PlayerPresenceLane.Identity, data));
        loop.RegisterGuarded<PresenceRoundChunkCommand>(
            static data => PresenceRoundChunkCommandCodec.TryRead(data, out _, out _),
            (source, data) => inbox.Enqueue(PlayerPresenceLane.Round, data));
        loop.RegisterGuarded<PresenceProofBatchCommand>(
            static data => PresenceProofBatchCommandCodec.TryRead(data, out _, out _),
            (source, data) =>
            {
                if (data.Length > PresenceProofBatchCommandCodec.HeaderSize)
                    inbox.Enqueue(PlayerPresenceLane.Proof, data);
            });

        cancellation = new();
        inbox.Open();
        var cancellationToken = cancellation.Token;
        workerThread = new Thread(() => RunWorker(cancellationToken))
        {
            IsBackground = true,
            Name = "Craftdig presence verifier",
        };
        challengeThread = new Thread(() => challenges.Run(cancellationToken))
        {
            IsBackground = true,
            Name = "Craftdig presence challenges",
        };
        workerThread.Start();
        challengeThread.Start();
    }

    public void Stop()
    {
        inbox.Close();
        cancellation?.Cancel();
        challengeThread?.Join();
        workerThread?.Join();
        cancellation?.Dispose();
        cancellation = null;
        challengeThread = null;
        workerThread = null;

        challenges.Clear();
        rounds.Clear();
        ledger.Clear();
        verifier.Clear();
        roster.Clear();
        log.Info("Client presence security stopped and all identity evidence was cleared");
    }

    private void RunWorker(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && socket.Connected)
            {
                bool worked = ProcessRoundMessage();
                worked |= ProcessIdentityMessage(cancellationToken);
                worked |= ProcessProofMessage();

                int verificationBudget = 128;
                while (verificationBudget-- > 0 && verifier.VerifyNext())
                    worked = true;

                verifier.SignRounds();
                roster.PublishIfDue(Stopwatch.GetTimestamp(), ledger, rounds);
                if (!worked)
                    inbox.WaitForWork(WorkerPollInterval);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            log.Warn("Presence verifier worker failed with {0}; disconnecting", e.GetType().Name);
            socket.Disconnect();
        }
    }

    private bool ProcessRoundMessage()
    {
        if (!inbox.TryDequeue(PlayerPresenceLane.Round, out var body))
            return false;
        if (!PresenceRoundChunkCommandCodec.TryRead(body, out var header, out var recordBytes))
        {
            log.Warn("Queued presence round chunk failed a second decode; bytes: {0}", body.Length);
            return true;
        }

        rounds.AcceptChunk(header, recordBytes, challenges.Snapshot(), Stopwatch.GetTimestamp());
        return true;
    }

    private bool ProcessIdentityMessage(CancellationToken cancellationToken)
    {
        if (!inbox.TryDequeue(PlayerPresenceLane.Identity, out var rawTicket))
            return false;
        if (identitySession.ServerContext == null)
        {
            return true;
        }

        try
        {
            var ticket = identityTrust.Validate(rawTicket, identitySession.ServerContext, out _, cancellationToken);
            if (ticket != null)
                AcceptTicket(ticket, rawTicket, Stopwatch.GetTimestamp());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        return true;
    }

    private bool ProcessProofMessage()
    {
        if (!inbox.TryDequeue(PlayerPresenceLane.Proof, out var body))
            return false;
        if (!PresenceProofBatchCommandCodec.TryRead(body, out var roundHash, out var recordBytes))
        {
            log.Warn("Queued presence proof batch failed a second decode; bytes: {0}", body.Length);
            return true;
        }

        var round = rounds.Find(roundHash);
        if (round == null || round.Invalid)
        {
            return true;
        }

        int recordCount = WireRecords.Count<PresenceProofRecord>(recordBytes);
        for (int i = 0; i < recordCount; i++)
        {
            if (WireRecords.TryReadAt(recordBytes, i, out PresenceProofRecord proof))
                rounds.RegisterProofRecord(roundHash, proof, ledger, roster);
        }

        return true;
    }

    private void AcceptTicket(ValidatedIdentityTicket ticket, ReadOnlySpan<byte> rawTicket, long activityTimestamp)
    {
        var change = ledger.Add(ticket, rawTicket);
        foreach (Guid playerId in change.InvalidPlayers)
            roster.MarkInvalid(playerId);
        if (change.EvictedTicket is { } evicted)
            rounds.DropPendingProofs(evicted);
        if (!change.Stored)
            return;

        if (rounds.HasDuplicateProofForTicket(ticket.TicketHash))
            roster.MarkInvalid(ticket.PlayerId);
        roster.Touch(ticket.PlayerId, activityTimestamp);
    }

    internal void AddTicketForTest(ValidatedIdentityTicket ticket, long activityTimestamp) =>
        AcceptTicket(ticket, ticket.RawTicket, activityTimestamp);

    internal int StoredPlayerCount => ledger.StoredPlayerCount;

    internal void PruneInactiveIdentities(long monotonicNow) => roster.PruneInactive(monotonicNow, ledger, rounds);
}
