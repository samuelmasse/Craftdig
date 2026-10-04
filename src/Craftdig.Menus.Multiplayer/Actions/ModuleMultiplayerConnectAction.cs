namespace Craftdig;

[Module]
public class ModuleMultiplayerConnectAction
{
    private readonly Log log;
    private readonly AppClientOptions clientOptions;
    private readonly ModuleMultiplayerAuthenticator authenticator;
    private ServerAddress? address;
    private volatile Thread? thread;
    private PendingConnection pending;
    private CancellationTokenSource? cancellation;
    private Exception? exception;

    public ServerAddress? Address => address;
    public bool Connecting => thread != null;
    public Exception? Exception => exception;

    public ModuleMultiplayerConnectAction(
        Log log,
        RootUnload unload,
        AppClientOptions clientOptions,
        ModuleMultiplayerAuthenticator authenticator)
    {
        this.log = log;
        this.clientOptions = clientOptions;
        this.authenticator = authenticator;
        unload.Add(Cancel);
    }

    public void Start(ServerAddress address)
    {
        log.Info("Starting multiplayer connection to {0}:{1}", address.Host, address.Port);
        Cancel();
        this.address = address;
        exception = null;
        cancellation = new();

        thread = new Thread(Connect)
        {
            IsBackground = true,
            Name = "Craftdig multiplayer connect",
        };

        thread.Start();
    }

    public void Cancel()
    {
        cancellation?.Cancel();
        pending.Socket?.Disconnect();
        pending.Tcp?.Dispose();
        thread?.Join();
        ClearUnclaimedConnection();
        cancellation?.Dispose();
        cancellation = null;
    }

    private void Connect()
    {
        try
        {
            EstablishConnection();
            pending.Socket = new PlayerSocket(log, pending.Tcp!, pending.Stream!);
            authenticator.Authenticate(pending.Socket, pending.Identity!, cancellation!.Token);
        }
        catch (Exception e)
        {
            log.Warn("Multiplayer connection failed during setup or authentication with {0}", e.GetType().Name);
            exception = e;
        }

        if (exception != null || cancellation!.IsCancellationRequested)
        {
            pending.Socket?.Disconnect();
            authenticator.Stop();
            ClearUnclaimedConnection();
        }

        thread = null;
    }

    public bool TryTakeConnection(
        [NotNullWhen(true)] out PlayerSocket? connectedSocket,
        [NotNullWhen(true)] out PlayerIdentitySession? connectedIdentity)
    {
        if (thread != null || exception != null || pending.Socket == null || pending.Identity == null)
        {
            connectedSocket = null;
            connectedIdentity = null;
            return false;
        }

        connectedSocket = pending.Socket;
        connectedIdentity = pending.Identity;
        pending.Tcp = null;
        pending.Stream = null;
        pending.Socket = null;
        pending.Identity = null;
        cancellation?.Dispose();
        cancellation = null;
        return true;
    }

    private void EstablishConnection()
    {
        var target = address ?? throw new InvalidOperationException("No multiplayer server address was selected.");
        pending.Tcp = new TcpClient { NoDelay = true };
        pending.Tcp.ConnectAsync(target.Host, target.Port, cancellation!.Token).GetAwaiter().GetResult();

        if (clientOptions.UseRawTcp)
        {
            log.Warn("Using raw TCP development transport; server identity and player Identity are unverified");
            pending.Stream = pending.Tcp.GetStream();
            pending.Identity = PlayerIdentitySession.CreateUnverified();
            if (clientOptions.NoAuthUser == null)
            {
                log.Warn("Rejecting raw TCP connection because no development no-auth username is configured");
                throw new InvalidOperationException("Raw TCP is unverified and requires a development no-auth username.");
            }
            return;
        }

        pending.Stream = ClientTls.Connect(log, pending.Tcp, target.Host);
        if (!ServerContext.TryCreate(target.Host, target.Port, out var serverContext))
        {
            log.Warn("Rejecting multiplayer address because its server context is not canonical");
            throw new InvalidDataException("The selected multiplayer server address is not canonicalizable.");
        }

        if (clientOptions.NoAuthUser != null)
        {
            log.Warn("Using TLS development no-auth mode; player Identity is unverified");
            pending.Identity = PlayerIdentitySession.CreateUnverified(serverContext);
            return;
        }

        pending.Identity = PlayerIdentitySession.CreateAuthenticated(serverContext);
    }

    private void ClearUnclaimedConnection()
    {
        pending.Socket?.Disconnect();
        pending.Socket?.ReleaseState();
        pending.Stream?.Dispose();
        pending.Tcp?.Dispose();
        pending.Identity?.Dispose();
        pending.Socket = null;
        pending.Stream = null;
        pending.Tcp = null;
        pending.Identity = null;
    }

    private struct PendingConnection
    {
        public TcpClient? Tcp;
        public Stream? Stream;
        public PlayerSocket? Socket;
        public PlayerIdentitySession? Identity;
    }
}
