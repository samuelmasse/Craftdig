namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerAuthenticator(
    Log log,
    AppClientOptions clientOptions,
    ModuleIdentityTicketClient identityTickets)
{
    private static readonly TimeSpan ExchangeTimeout = TimeSpan.FromSeconds(20);

    public void Authenticate(NetSocket socket, PlayerIdentitySession identitySession, CancellationToken cancellationToken)
    {
        var loop = new NetLoop(log);
        using var pushCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var resultEvent = ListenForResult(loop);
        var nonceListener = new NonceListener(log);
        loop.RegisterDecoded<ReadyAuthCommand, Nonce256>(ReadyAuthCommandCodec.TryRead, nonceListener.Receive);
        var (loopThread, pushThread) = StartExchangeThreads(loop, socket, pushCancellation);

        var stopwatch = Stopwatch.StartNew();
        var pendingTicket = SendCredentials(socket, identitySession, nonceListener, loopThread, stopwatch, cancellationToken);

        while (loopThread.IsAlive && !resultEvent.IsSet && stopwatch.Elapsed < ExchangeTimeout)
            Wait(cancellationToken);

        if (!resultEvent.IsSet)
        {
            throw stopwatch.Elapsed >= ExchangeTimeout
                ? new TimeoutException("Timed out while authenticating with the multiplayer server.")
                : new InvalidDataException("The multiplayer server rejected authentication.");
        }

        if (pendingTicket != null)
        {
            identitySession.InstallTicket(pendingTicket);
        }

        log.Info("Multiplayer server accepted authentication");

        loopThread.Join();
        pushThread.Join();
    }

    private static (Thread Receive, Thread Push) StartExchangeThreads(
        NetLoop loop,
        NetSocket socket,
        CancellationTokenSource pushCancellation)
    {
        var receive = new Thread(() =>
        {
            try
            {
                loop.Run(socket);
            }
            catch
            {
            }
            finally
            {
                pushCancellation.Cancel();
            }
        })
        {
            IsBackground = true,
            Name = "Craftdig authentication receive",
        };
        var push = new Thread(() =>
        {
            try
            {
                socket.Push(pushCancellation.Token);
            }
            catch
            {
            }
        })
        {
            IsBackground = true,
            Name = "Craftdig authentication send",
        };

        receive.Start();
        push.Start();
        return (receive, push);
    }

    private ValidatedIdentityTicket? SendCredentials(
        NetSocket socket,
        PlayerIdentitySession identitySession,
        NonceListener nonceListener,
        Thread loopThread,
        Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        if (!identitySession.IdentityEnabled)
        {
            socket.Send<NoAuthCommand, byte>(Encoding.UTF8.GetBytes(clientOptions.NoAuthUser!));
            return null;
        }

        var pendingTicket = identityTickets.Request(identitySession, cancellationToken);
        var nonce = AcquireNonce(socket, stopwatch, nonceListener, loopThread, cancellationToken);
        var signature = identitySession.SignAuthentication(pendingTicket, nonce);
        byte[] completeAuth = new byte[pendingTicket.RawTicketSize + P256Signature.Size];
        if (!CompleteAuthCommandCodec.TryWrite(
                completeAuth,
                pendingTicket.RawTicket,
                signature,
                out int written) || written != completeAuth.Length)
            throw new InvalidDataException("Unable to encode the multiplayer proof-of-possession response.");

        socket.SendRaw<CompleteAuthCommand>(completeAuth);
        return pendingTicket;
    }

    private Nonce256 AcquireNonce(
        NetSocket socket,
        Stopwatch stopwatch,
        NonceListener nonceListener,
        Thread loopThread,
        CancellationToken cancellationToken)
    {
        socket.SendRaw<BeginAuthCommand>([]);
        Nonce256? nonce = nonceListener.Read();
        while (loopThread.IsAlive && nonce == null && stopwatch.Elapsed < ExchangeTimeout)
        {
            Wait(cancellationToken);
            nonce = nonceListener.Read();
        }

        if (nonce == null)
        {
            log.Warn("Multiplayer server did not provide a usable authentication challenge; receive loop alive: {0}", loopThread.IsAlive);
            throw new InvalidDataException("The multiplayer server did not provide a valid authentication nonce.");
        }

        return nonce.Value;
    }

    private sealed class NonceListener(Log log)
    {
        private readonly Lock gate = new();
        private Nonce256? nonce;

        public void Receive(NetSocket ns, Nonce256 parsed)
        {
            lock (gate)
            {
                if (nonce != null)
                {
                    log.Warn("Rejecting duplicate multiplayer authentication challenge");
                    ns.Disconnect();
                    return;
                }

                nonce = parsed;
            }
        }

        public Nonce256? Read()
        {
            lock (gate)
                return nonce;
        }
    }

    private static ManualResetEventSlim ListenForResult(NetLoop loop)
    {
        var resultEvent = new ManualResetEventSlim(false);
        loop.RegisterGuarded<ResultAuthCommand>(static data => data.IsEmpty, (ns, data) =>
        {
            resultEvent.Set();

            throw new OperationCanceledException();
        });
        return resultEvent;
    }

    private static void Wait(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        cancellationToken.WaitHandle.WaitOne(10);
    }
}
