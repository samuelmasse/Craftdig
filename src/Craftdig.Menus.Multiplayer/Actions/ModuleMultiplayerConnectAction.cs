namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerConnectAction(
    AppLog log,
    AppClientOptions clientOptions,
    ModuleMultiplayerAuthenticator authenticator)
{
    private ServerAddress? address;
    private volatile Thread? thread;
    private TcpClient? tcp;
    private Stream? stream;
    private NetSocket? socket;
    private PlayerIdentitySession? identitySession;
    private CancellationTokenSource? cancellation;
    private Exception? exception;

    public ServerAddress? Address => address;
    public bool Connecting => thread != null;
    public TcpClient? Tcp => tcp;
    public Stream? Stream => stream;
    public Exception? Exception => exception;

    public void Start(ServerAddress address)
    {
        log.Info("Starting multiplayer connection to {0}:{1}", address.Host, address.Port);
        while (thread != null)
        {
            Thread.Sleep(10);
        }

        ClearUnclaimedConnection();
        cancellation?.Dispose();
        this.address = address;
        exception = null;
        cancellation = new();

        thread = new Thread(() =>
        {
            try
            {
                EstablishConnection();
                socket = new NetSocket(log, tcp!, stream!);
                authenticator.Authenticate(socket, identitySession!, cancellation.Token);
            }
            catch (Exception e)
            {
                log.Warn("Multiplayer connection failed during setup or authentication with {0}", e.GetType().Name);
                Cancel();
                exception = e;
            }
            finally
            {
                thread = null;
            }
        })
        {
            IsBackground = true,
            Name = "Craftdig multiplayer connect",
        };

        thread.Start();
    }

    public void Cancel()
    {
        cancellation?.Cancel();
        ClearUnclaimedConnection();
    }

    public bool TryTakeConnection(
        [NotNullWhen(true)] out TcpClient? connectedTcp,
        [NotNullWhen(true)] out Stream? connectedStream,
        [NotNullWhen(true)] out PlayerIdentitySession? connectedIdentity)
    {
        if (thread != null || exception != null || tcp == null || stream == null || identitySession == null)
        {
            connectedTcp = null;
            connectedStream = null;
            connectedIdentity = null;
            return false;
        }

        connectedTcp = tcp;
        connectedStream = stream;
        connectedIdentity = identitySession;
        tcp = null;
        stream = null;
        socket = null;
        identitySession = null;
        cancellation?.Dispose();
        cancellation = null;
        return true;
    }

    private void EstablishConnection()
    {
        var target = address ?? throw new InvalidOperationException("No multiplayer server address was selected.");
        tcp = new TcpClient { NoDelay = true };
        tcp.Connect(target.Host, target.Port);

        if (clientOptions.UseRawTcp)
        {
            log.Warn("Using raw TCP development transport; server identity and player Identity are unverified");
            stream = tcp.GetStream();
            identitySession = PlayerIdentitySession.CreateUnverified();
            if (clientOptions.NoAuthUser == null)
            {
                log.Warn("Rejecting raw TCP connection because no development no-auth username is configured");
                throw new InvalidOperationException("Raw TCP is unverified and requires a development no-auth username.");
            }
            return;
        }

        stream = ClientTls.Connect(log, tcp, target.Host);
        if (!ServerContext.TryCreate(target.Host, target.Port, out var serverContext))
        {
            log.Warn("Rejecting multiplayer address because its server context is not canonical");
            throw new InvalidDataException("The selected multiplayer server address is not canonicalizable.");
        }

        if (clientOptions.NoAuthUser != null)
        {
            log.Warn("Using TLS development no-auth mode; player Identity is unverified");
            identitySession = PlayerIdentitySession.CreateUnverified(serverContext);
            return;
        }

        identitySession = PlayerIdentitySession.CreateAuthenticated(serverContext);
    }

    private void ClearUnclaimedConnection()
    {
        try { socket?.Disconnect(); } catch { }
        try { stream?.Dispose(); } catch { }
        try { tcp?.Dispose(); } catch { }
        identitySession?.Dispose();
        socket = null;
        stream = null;
        tcp = null;
        identitySession = null;
    }
}
