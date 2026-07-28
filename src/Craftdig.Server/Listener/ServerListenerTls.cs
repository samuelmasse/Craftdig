namespace Craftdig.Server;

[Server]
public class ServerListenerTls(
    Log log,
    ServerDefaults defaults,
    ServerConfig config,
    ServerListenerLoop listenerLoop,
    ServerLoadCertificateAction loadCertificateAction)
{
    private Thread? thread;
    private Action? stop;

    public void Start()
    {
        if (config.DisableTls ?? defaults.DisableTls)
            return;

        var cert = loadCertificateAction.Run();
        int port = 36676;

        (thread, stop) = listenerLoop.Run(port, (tcp) =>
        {
            var ssl = new SslStream(tcp.GetStream(), false);
            var opt = new SslServerAuthenticationOptions
            {
                ServerCertificate = cert,
                EnabledSslProtocols = SslProtocols.Tls13,
                ClientCertificateRequired = false,
                CertificateRevocationCheckMode = X509RevocationMode.Online
            };

            ssl.AuthenticateAsServer(opt);
            log.Info(
                "TLS server handshake succeeded for socket {0}; protocol: {1}, cipher: {2}",
                tcp.Client.RemoteEndPoint,
                ssl.SslProtocol,
                ssl.NegotiatedCipherSuite);

            return new(log, tcp, ssl)
            {
                IsTransportSecure = true,
            };
        });
        thread.Start();

        log.Info("Listening on TLS port {0}...", port);
    }

    public void Stop() => stop?.Invoke();

    public void Join()
    {
        thread?.Join();
        log.Info("Listener TLS stopped");
    }
}
