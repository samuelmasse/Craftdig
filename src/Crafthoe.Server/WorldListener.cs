namespace Crafthoe.Server;

[World]
public class WorldListener(NetLoop nloop, WorldLoadCertificateAction loadCertificateAction)
{
    public void Start()
    {
        int port = 36676;
        var cert = loadCertificateAction.Run();
        using var listener = new TcpListener(IPAddress.Any, port);
        listener.Start(10);

        Console.WriteLine($"Listening on port {port}...");

        while (true)
        {
            var tcp = listener.AcceptTcpClient();
            var ssl = new SslStream(tcp.GetStream(), false);
            var opt = new SslServerAuthenticationOptions
            {
                ServerCertificate = cert,
                EnabledSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12,
                ClientCertificateRequired = false,
                CertificateRevocationCheckMode = X509RevocationMode.Online
            };

            ssl.AuthenticateAsServer(opt);

            try
            {
                tcp.NoDelay = true;
                new Thread(() => ClientLoop(new(tcp, ssl))).Start();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Error in listener");
                Console.Error.WriteLine(e);
                Console.ResetColor();
            }
        }
    }

    private void ClientLoop(NetSocket socket)
    {
        Console.WriteLine($"Socket connected");

        try
        {
            nloop.Run(socket);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Error in socket");
            Console.Error.WriteLine(e);
            Console.ResetColor();
        }

        socket.Disconnect();

        Console.WriteLine($"Socket disconnected");
    }
}
