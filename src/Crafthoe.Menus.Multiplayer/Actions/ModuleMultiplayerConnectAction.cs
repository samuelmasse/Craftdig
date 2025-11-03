namespace Crafthoe.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerConnectAction
{
    private string? host;
    private int port;
    private Thread? thread;
    private TcpClient? tcp;
    private SslStream? ssl;
    private Exception? exception;

    public string? Host => host;
    public int Port => port;
    public bool Connecting => thread != null;
    public TcpClient? Tcp => tcp;
    public SslStream? Ssl => ssl;
    public Exception? Exception => exception;

    public void Start(string host, int port)
    {
        while (thread != null)
            Thread.Sleep(10);

        this.host = host;
        this.port = port;

        tcp = null;
        ssl = null;
        exception = null;

        thread = new Thread(() =>
        {
            try
            {
                tcp = new TcpClient();
                tcp.Connect(host, port);

                ssl = new SslStream(tcp.GetStream(), false, (sender, certificate, chain, errors) =>
                {
                    if (host == "localhost")
                        return true;

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

                var opt = new SslClientAuthenticationOptions
                {
                    TargetHost = host,
                    EnabledSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                };
                ssl.AuthenticateAsClient(opt);
            }
            catch (Exception e)
            {
                try { ssl?.Dispose(); } catch { }
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

        try { ssl?.Dispose(); } catch { }
        try { tcp?.Dispose(); } catch { }
    }
}
