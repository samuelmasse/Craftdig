namespace Crafthoe.Server;

[World]
public class WorldCreateDevCertificateAction
{
    public X509Certificate2 Run()
    {
        Console.WriteLine("Generating dev certificate");

        using var ca = CreateCACertificate();
        using var leaf = CreateLeaftCertificate(ca);

        var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var pfx = new X509Certificate2Collection(new X509Certificate2[] { leaf, ca })
            .Export(X509ContentType.Pkcs12, password)!;

        return X509CertificateLoader.LoadPkcs12(pfx, password,
            X509KeyStorageFlags.UserKeySet |
            X509KeyStorageFlags.PersistKeySet |
            X509KeyStorageFlags.Exportable);
    }

    private X509Certificate2 CreateCACertificate()
    {
        using var rsa = RSA.Create(4096);
        var req = new CertificateRequest("CN=Issuer", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(true, false, 0, true));
        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        req.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

        return req.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(5));
    }

    private X509Certificate2 CreateLeaftCertificate(X509Certificate2 ca)
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));
        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
        req.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.1")], false));

        var san = new SubjectAlternativeNameBuilder();
        san.AddDnsName("localhost");
        san.AddIpAddress(IPAddress.Loopback);
        san.AddIpAddress(IPAddress.IPv6Loopback);
        req.CertificateExtensions.Add(san.Build());
        req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

        using var unsigned = req.Create(
            ca,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1),
            RandomNumberGenerator.GetBytes(4));

        return unsigned.CopyWithPrivateKey(rsa);
    }
}
