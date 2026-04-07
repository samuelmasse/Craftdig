namespace Craftdig.Menus.Multiplayer;

public static class ClientTls
{
    public static SslStream Connect(TcpClient tcp, string host)
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

        ssl.AuthenticateAsClient(new SslClientAuthenticationOptions
        {
            TargetHost = host,
            EnabledSslProtocols = SslProtocols.Tls13,
            CertificateRevocationCheckMode = X509RevocationMode.Online
        });

        return ssl;
    }
}
