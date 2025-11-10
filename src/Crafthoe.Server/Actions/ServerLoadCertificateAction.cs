namespace Crafthoe.Server;

[Server]
public class ServerLoadCertificateAction(ServerCreateDevCertificateAction createDevCertificateAction)
{
    public X509Certificate2 Run()
    {
        var certPath = Environment.GetEnvironmentVariable("CRAFTHOE_CERT_PATH");
        var keyPath = Environment.GetEnvironmentVariable("CRAFTHOE_CERT_KEY_PATH");
        var password = Environment.GetEnvironmentVariable("CRAFTHOE_CERT_PASSWORD");

        if (password != null)
        {
            if (certPath == null)
                throw new Exception("TLS password defined without cert path");

            return X509CertificateLoader.LoadPkcs12FromFile(certPath, password);
        }
        else if (keyPath != null)
        {
            if (certPath == null)
                throw new Exception("TLS key path defined without cert path");

            return X509Certificate2.CreateFromPemFile(certPath, keyPath);
        }
        else
        {
            if (certPath != null)
                throw new Exception("TLS cert path defined without password or key");

            return createDevCertificateAction.Run();
        }
    }
}
