namespace Crafthoe.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerConnectAction(AppClientOptions clientOptions)
{
    private string? host;
    private int port;
    private Thread? thread;
    private TcpClient? tcp;
    private Stream? stream;
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
                tcp = new TcpClient();
                tcp.Connect(host, port);

                if (!clientOptions.UseRawTcp)
                {
                    var ssl = new SslStream(tcp.GetStream(), false, (sender, certificate, chain, errors) =>
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

                    var opt = new SslClientAuthenticationOptions
                    {
                        TargetHost = host,
                        EnabledSslProtocols = SslProtocols.Tls13,
                        CertificateRevocationCheckMode = X509RevocationMode.Online
                    };
                    ssl.AuthenticateAsClient(opt);
                }
                else stream = tcp.GetStream();
            }
            catch (Exception e)
            {
                try { stream?.Dispose(); } catch { }
                try { tcp?.Dispose(); } catch { }
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

        try { stream?.Dispose(); } catch { }
        try { tcp?.Dispose(); } catch { }
    }
}
