namespace Crafthoe.Server;

[Server]
public class ServerLoadCertificateAction(ServerConfig config, ServerCreateDevCertificateAction createDevCertificateAction)
{
    public X509Certificate2 Run()
    {
        if (config.KeyPath != null)
        {
            if (config.CertPath == null)
                throw new Exception("TLS key path defined without cert path");

            return X509Certificate2.CreateFromPemFile(config.CertPath, config.KeyPath);
        }
        else
        {
            if (config.CertPath != null)
                throw new Exception("TLS cert path defined without key");

            return createDevCertificateAction.Run();
        }
    }
}
