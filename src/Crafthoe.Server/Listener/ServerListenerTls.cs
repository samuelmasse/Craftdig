namespace Crafthoe.Server;

[Server]
public class ServerListenerTls(ServerListenerLoop listenerLoop, ServerLoadCertificateAction loadCertificateAction)
{
    private Thread? thread;

    public void Start()
    {
        var cert = loadCertificateAction.Run();
        int port = 36676;

        thread = listenerLoop.Run(port, (tcp) =>
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

    public void Join() => thread?.Join();
}
