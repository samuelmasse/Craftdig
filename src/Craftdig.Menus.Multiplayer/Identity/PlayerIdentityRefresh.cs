namespace Craftdig;

[Player]
public class PlayerIdentityRefresh(
    Log log,
    ModuleIdentityTicketClient identityTickets,
    PlayerIdentitySession identitySession,
    PlayerNetLoop loop,
    PlayerSocket socket)
{
    private static readonly TimeSpan RefreshAge = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan RefreshRetryDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ServerExchangeTimeout = TimeSpan.FromSeconds(10);

    private readonly Lock gate = new();
    private readonly ManualResetEventSlim nonceReady = new(false);
    private readonly ManualResetEventSlim resultReady = new(false);
    private CancellationTokenSource? cancellation;
    private Thread? thread;
    private Nonce256? pendingNonce;
    private bool awaitingNonce;
    private bool awaitingResult;

    public void Start()
    {
        if (!identitySession.IdentityEnabled)
        {
            return;
        }
        if (thread != null)
            throw new InvalidOperationException("The Identity ticket refresh worker is already running.");

        loop.RegisterDecoded<ReadyAuthCommand, Nonce256>(ReadyAuthCommandCodec.TryRead, ReceiveNonce);
        loop.RegisterGuarded<ResultAuthCommand>(static data => data.IsEmpty, ReceiveResult);
        cancellation = new();
        thread = new Thread(() => Run(cancellation.Token))
        {
            IsBackground = true,
            Name = "Craftdig Identity refresh",
        };
        thread.Start();
    }

    public void Stop()
    {
        cancellation?.Cancel();
        thread?.Join();
        cancellation?.Dispose();
        cancellation = null;
        thread = null;
    }

    private void Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && socket.Connected)
        {
            var currentTicket = identitySession.GetCurrentTicket();
            if (currentTicket == null)
            {
                log.Warn("Disconnecting because the authenticated connection has no current Identity ticket to refresh");
                socket.Disconnect();
                return;
            }

            if (WaitUntilRefreshTime(currentTicket, cancellationToken))
                return;

            switch (AcquireTicket(currentTicket, cancellationToken, out var refreshedTicket))
            {
                case RefreshStep.Stop:
                    return;
                case RefreshStep.Retry:
                    continue;
            }

            if (!TryInstallTicket(refreshedTicket!, cancellationToken))
                return;
        }
    }

    private bool WaitUntilRefreshTime(ValidatedIdentityTicket currentTicket, CancellationToken cancellationToken)
    {
        var refreshAt = currentTicket.IssuedAt + RefreshAge + TimeSpan.FromSeconds(RandomNumberGenerator.GetInt32(61) - 30);
        var delay = refreshAt - DateTimeOffset.UtcNow;
        return delay > TimeSpan.Zero && cancellationToken.WaitHandle.WaitOne(delay);
    }

    private RefreshStep AcquireTicket(
        ValidatedIdentityTicket currentTicket,
        CancellationToken cancellationToken,
        out ValidatedIdentityTicket? refreshedTicket)
    {
        refreshedTicket = null;
        try
        {
            refreshedTicket = identityTickets.Request(identitySession, cancellationToken);
            return RefreshStep.Acquired;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RefreshStep.Stop;
        }
        catch (Exception e)
        {
            if (DateTimeOffset.UtcNow >= currentTicket.ExpiresAt)
            {
                log.Warn("Identity ticket refresh failed with {0} after the current ticket expired; disconnecting", e.GetType().Name);
                socket.Disconnect();
                return RefreshStep.Stop;
            }

            log.Warn(
                "Identity ticket refresh failed with {0}; current ticket remains active until {1}",
                e.GetType().Name, currentTicket.ExpiresAt);
            return cancellationToken.WaitHandle.WaitOne(RefreshRetryDelay) ? RefreshStep.Stop : RefreshStep.Retry;
        }
    }

    private bool TryInstallTicket(ValidatedIdentityTicket refreshedTicket, CancellationToken cancellationToken)
    {
        try
        {
            RefreshServerAuthentication(refreshedTicket, cancellationToken);
            identitySession.InstallTicket(refreshedTicket);
            log.Info("Multiplayer server accepted and installed the refreshed Identity ticket");
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception e)
        {
            log.Warn("Multiplayer server Identity ticket refresh failed with {0}; disconnecting", e.GetType().Name);
            socket.Disconnect();
            return false;
        }
    }

    private enum RefreshStep
    {
        Acquired,
        Retry,
        Stop,
    }

    private void RefreshServerAuthentication(ValidatedIdentityTicket ticket, CancellationToken cancellationToken)
    {
        lock (gate)
        {
            pendingNonce = null;
            awaitingNonce = true;
            awaitingResult = false;
            nonceReady.Reset();
            resultReady.Reset();
        }

        socket.SendRaw<BeginAuthCommand>([]);
        if (!nonceReady.Wait(ServerExchangeTimeout, cancellationToken))
        {
            log.Warn("Timed out waiting for multiplayer ticket refresh challenge");
            throw new TimeoutException("The multiplayer server did not return a refresh nonce.");
        }

        Nonce256 nonce;
        lock (gate)
        {
            awaitingNonce = false;
            nonce = pendingNonce ?? throw new InvalidDataException("The multiplayer server returned an invalid refresh nonce.");
            awaitingResult = true;
        }

        var signature = identitySession.SignAuthentication(ticket, nonce);
        byte[] completeAuth = new byte[ticket.RawTicketSize + P256Signature.Size];
        if (!CompleteAuthCommandCodec.TryWrite(
                completeAuth,
                ticket.RawTicket,
                signature,
                out int written) || written != completeAuth.Length)
            throw new InvalidDataException("Unable to encode the refreshed proof-of-possession response.");

        socket.SendRaw<CompleteAuthCommand>(completeAuth);
        if (!resultReady.Wait(ServerExchangeTimeout, cancellationToken))
        {
            log.Warn("Timed out waiting for multiplayer server to confirm refreshed Identity ticket");
            throw new TimeoutException("The multiplayer server did not confirm Identity ticket refresh.");
        }

        lock (gate)
            awaitingResult = false;
    }

    private void ReceiveNonce(NetSocket source, Nonce256 nonce)
    {
        bool unexpected;
        bool duplicate;
        lock (gate)
        {
            unexpected = !awaitingNonce;
            duplicate = pendingNonce != null;
            if (!unexpected && !duplicate)
            {
                pendingNonce = nonce;
                nonceReady.Set();
            }
        }

        if (unexpected || duplicate)
        {
            log.Warn("Rejecting multiplayer ticket refresh challenge; unexpected: {0}, duplicate: {1}", unexpected, duplicate);
            source.Disconnect();
        }
    }

    private void ReceiveResult(NetSocket source, ReadOnlySpan<byte> data)
    {
        bool unexpected;
        bool duplicate;
        lock (gate)
        {
            unexpected = !awaitingResult;
            duplicate = resultReady.IsSet;
            if (!unexpected && !duplicate)
                resultReady.Set();
        }

        if (unexpected || duplicate)
        {
            log.Warn("Rejecting multiplayer ticket refresh result; unexpected: {0}, duplicate: {1}", unexpected, duplicate);
            source.Disconnect();
        }
    }
}
