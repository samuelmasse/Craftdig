namespace Craftdig.Menus.Multiplayer;

using System.Net.Http.Headers;
using System.Net.Http.Json;

[Module]
public class ModuleMultiplayerConnectAction(AppLog log, AppClientOptions clientOptions, ModuleMultiplayerCredentials credentials)
{
    private string? host;
    private int port;
    private Thread? thread;
    private TcpClient? tcp;
    private Stream? stream;
    private NetSocket? socket;
    private Exception? exception;

    public string? Host => host;
    public int Port => port;
    public bool Connecting => thread != null;
    public TcpClient? Tcp => tcp;
    public Stream? Stream => stream;
    public Exception? Exception => exception;

    public void Start(string host, int port)
    {
        while (thread != null)
            Thread.Sleep(10);

        this.host = host;
        this.port = port;

        tcp = null;
        stream = null;
        exception = null;

        thread = new Thread(() =>
        {
            try
            {
                EstablishConnection();
                AuthenticateConnection();
            }
            catch (Exception e)
            {
                Cancel();
                exception = e;
            }
            finally
            {
                thread = null;
            }
        });

        thread.Start();
    }

    public void Cancel()
    {
        if (thread == null)
            return;

        try { socket?.Disconnect(); } catch { }
        try { stream?.Dispose(); } catch { }
        try { tcp?.Dispose(); } catch { }
    }

    private void EstablishConnection()
    {
        tcp = new TcpClient() { NoDelay = true };
        tcp.Connect(host!, port);

        if (clientOptions.UseRawTcp)
            stream = tcp.GetStream();
        else ConnectTls();
    }

    private void ConnectTls()
    {
        var ssl = new SslStream(tcp!.GetStream(), false, (sender, certificate, chain, errors) =>
        {
            if (host == "127.0.0.1")
                errors &= ~SslPolicyErrors.RemoteCertificateChainErrors;

            if (errors == SslPolicyErrors.None)
                return true;

            if (errors == SslPolicyErrors.RemoteCertificateChainErrors && chain != null)
            {
                foreach (var s in chain.ChainStatus)
                {
                    if (s.Status != X509ChainStatusFlags.NoError &&
                        s.Status != X509ChainStatusFlags.RevocationStatusUnknown)
                        return false;
                }

                return true;
            }

            return false;
        });
        stream = ssl;

        var opt = new SslClientAuthenticationOptions()
        {
            TargetHost = host,
            EnabledSslProtocols = SslProtocols.Tls13,
            CertificateRevocationCheckMode = X509RevocationMode.Online
        };
        ssl.AuthenticateAsClient(opt);
    }

    private void AuthenticateConnection()
    {
        var loop = new NetLoop(log);
        socket = new NetSocket(log, tcp!, stream!);

        using var ct = new CancellationTokenSource();
        var loopThread = new Thread(() => { try { loop.Run(socket); } catch { } finally { ct.Cancel(); } });
        var pushThread = new Thread(() => { try { socket.Push(ct.Token); } catch { } });
        var resultEvent = ListenForResult(loop);

        loopThread.Start();
        pushThread.Start();

        var sw = Stopwatch.StartNew();
        var timeout = TimeSpan.FromSeconds(10);

        if (clientOptions.NoAuthUser == null)
        {
            var nonce = AcquireNonce(sw, timeout, loop, loopThread);
            var jwt = AcquireJwt(nonce);

            socket.Send<CompleteAuthCommand, byte>(Encoding.UTF8.GetBytes(jwt));
        }
        else socket.Send<NoAuthCommand, byte>(Encoding.UTF8.GetBytes(clientOptions.NoAuthUser));

        while (loopThread.IsAlive && sw.Elapsed < timeout) { Thread.Sleep(10); }

        if (!resultEvent.IsSet)
        {
            if (sw.Elapsed >= timeout)
                throw new Exception("Timed out");
            else throw new Exception("Failed to authenticate");
        }

        loopThread.Join();
        pushThread.Join();
    }

    private string AcquireNonce(Stopwatch sw, TimeSpan timeout, NetLoop loop, Thread loopThread)
    {
        string? nonce = null;

        loop.Register<ReadyAuthCommand, byte>(data =>
        {
            log.Warn("Nonce {0}", nonce);
            nonce = Encoding.UTF8.GetString(data);
        });

        socket!.Send<BeginAuthCommand>();
        while (loopThread.IsAlive && nonce == null && sw.Elapsed < timeout) { Thread.Sleep(10); }

        if (nonce == null)
            throw new Exception("Failed to acquire nonce");

        return nonce;
    }

    private string AcquireJwt(string nonce)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", credentials.GetFreshToken());

        log.Warn(credentials.GetFreshToken());

        var postTask = http.PostAsJsonAsync($"https://craftdig.io/api/GetToken", new { host, nonce });
        postTask.Wait();

        var bodyTask = postTask.Result.Content.ReadFromJsonAsync<GetTokenResponse>();
        bodyTask.Wait();

        var body = bodyTask.Result;

        if (body == null || body?.Jwt == null)
            throw new Exception("Failed to obtain JWT");

        log.Warn("Token {0}", body?.Jwt);
        return body!.Jwt;
    }

    private ManualResetEventSlim ListenForResult(NetLoop loop)
    {
        var resultEvent = new ManualResetEventSlim(false);

        loop.Register<ResultAuthCommand>(() =>
        {
            resultEvent.Set();

            // Force terminate the net loop
            throw new Exception();
        });

        return resultEvent;
    }

    private record GetTokenResponse(string Jwt);
}
