namespace Craftdig;

public static class ClientTls
{
    public static SslStream Connect(Log log, TcpClient tcp, string host)
    {
        bool connectingToIp = IPAddress.TryParse(host, out _);
        var ssl = new SslStream(tcp.GetStream(), false, (sender, certificate, chain, errors) =>
        {
            if (connectingToIp)
            {
                const SslPolicyErrors ignoredErrors =
                    SslPolicyErrors.RemoteCertificateNameMismatch |
                    SslPolicyErrors.RemoteCertificateChainErrors;
                if ((errors & ignoredErrors) != 0)
                {
                    log.Warn(
                        "Accepting TLS certificate without server identity verification for literal IP multiplayer host {0}",
                        host);
                }
                errors &= ~ignoredErrors;
            }

            if (errors == SslPolicyErrors.None)
            {
                return true;
            }

            if (errors == SslPolicyErrors.RemoteCertificateChainErrors && chain != null)
            {
                foreach (var s in chain.ChainStatus)
                {
                    if (s.Status != X509ChainStatusFlags.NoError &&
                        s.Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        log.Warn(
                            "Rejecting TLS certificate for multiplayer host {0}; chain status: {1}",
                            host,
                            s.Status);
                        return false;
                    }
                }

                log.Warn(
                    "Accepting TLS certificate for multiplayer host {0} with revocation status unavailable",
                    host);
                return true;
            }

            log.Warn(
                "Rejecting TLS certificate for multiplayer host {0}; policy errors: {1}",
                host,
                errors);
            return false;
        });

        ssl.AuthenticateAsClient(new SslClientAuthenticationOptions
        {
            TargetHost = host,
            EnabledSslProtocols = SslProtocols.Tls13,
            CertificateRevocationCheckMode = X509RevocationMode.Online
        });

        log.Info(
            "TLS handshake succeeded with multiplayer host {0}; protocol: {1}, cipher: {2}",
            host,
            ssl.SslProtocol,
            ssl.NegotiatedCipherSuite);
        return ssl;
    }
}
