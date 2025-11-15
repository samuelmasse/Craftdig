namespace Crafthoe.Server;

[Server]
public class ServerListenerTls(
    ServerDefaults defaults,
    ServerListenerLoop listenerLoop,
    ServerLoadCertificateAction loadCertificateAction)
{
    private Thread? thread;
    private Action? stop;

    public void Start()
    {
        if (defaults.DisableTls)
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

            return new(tcp, ssl);
        });
        thread.Start();

        Console.WriteLine($"Listening on TLS port {port}...");
    }

    public void Stop() => stop?.Invoke();

    public void Join()
    {
        thread?.Join();
        Console.WriteLine("Listener TLS stopped");
    }
}
