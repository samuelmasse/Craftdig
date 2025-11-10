namespace Crafthoe.Server;

[Server]
public class ServerListenerTls(ServerListenerLoop listenerLoop, ServerLoadCertificateAction loadCertificateAction)
{
    private Thread? thread;
    private Action? stop;

    public void Start()
    {
        var cert = loadCertificateAction.Run();
        int port = 36676;

        (thread, stop) = listenerLoop.Run(port, (tcp) =>
        {
            var ssl = new SslStream(tcp.GetStream(), false);
            var opt = new SslServerAuthenticationOptions
            {
                ServerCertificate = cert,
                EnabledSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12,
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
        Console.WriteLine("Listener TLS stopped");
        thread?.Join();
    }
}
