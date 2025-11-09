namespace Crafthoe.Server;

[Server]
public class ServerLoadCertificateAction(ServerCreateDevCertificateAction createDevCertificateAction)
{
    public X509Certificate2 Run()
    {
        var path = Environment.GetEnvironmentVariable("CRAFTHOE_CERT_PATH");
        var password = Environment.GetEnvironmentVariable("CRAFTHOE_CERT_PASSWORD");

        if (path == null || password == null)
            return createDevCertificateAction.Run();
        else return X509CertificateLoader.LoadPkcs12FromFile(path, password);
    }
}
