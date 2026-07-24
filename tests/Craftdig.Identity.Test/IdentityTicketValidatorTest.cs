namespace Craftdig.Identity.Test;

[TestClass]
public sealed class IdentityTicketValidatorTest
{
    [TestMethod]
    public void ValidRs256Ticket_ForAllowedContext_IsAccepted()
    {
        using var signingKey = RSA.Create(2048);
        using var proofKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        byte[] ticket = CreateTicket(signingKey, proofKey, "craftdig-auth", "localhost", 36676);
        var validator = CreateValidator(signingKey, [("localhost", 36676)]);

        var validated = validator.Validate(ticket, out var failure);

        Assert.IsNotNull(validated);
        Assert.AreEqual(IdentityTicketFailure.None, failure);
        Assert.AreEqual("localhost", validated.ServerContext.Host);
        Assert.AreEqual((ushort)36676, validated.ServerContext.Port);
    }

    [TestMethod]
    public void ValidRs256Ticket_WithoutAllowedContexts_FailsClosed()
    {
        using var signingKey = RSA.Create(2048);
        using var proofKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        byte[] ticket = CreateTicket(signingKey, proofKey, "craftdig-auth", "localhost", 36676);
        var validator = CreateValidator(signingKey, []);

        Assert.IsNull(validator.Validate(ticket, out var failure));
        Assert.AreEqual(IdentityTicketFailure.ContextNotAllowed, failure);
    }

    [TestMethod]
    public void ValidRs256Ticket_ForDifferentAllowedContext_ReportsContextRejection()
    {
        using var signingKey = RSA.Create(2048);
        using var proofKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        byte[] ticket = CreateTicket(signingKey, proofKey, "craftdig-auth", "localhost", 36676);
        var validator = CreateValidator(signingKey, [("127.0.0.1", 36676)]);

        Assert.IsNull(validator.Validate(ticket, out var failure));
        Assert.AreEqual(IdentityTicketFailure.ContextNotAllowed, failure);
    }

    [TestMethod]
    public void ExpectedContextOverload_RequiresExactConnectionContext()
    {
        using var signingKey = RSA.Create(2048);
        using var proofKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        byte[] ticket = CreateTicket(signingKey, proofKey, "craftdig-auth", "localhost", 36676);
        var validator = CreateValidator(signingKey, []);
        Assert.IsTrue(ServerContext.TryCreate("localhost", 36676, out var matching));
        Assert.IsTrue(ServerContext.TryCreate("localhost", 36677, out var wrongPort));

        Assert.IsNotNull(validator.Validate(ticket, matching, out var matchingFailure));
        Assert.AreEqual(IdentityTicketFailure.None, matchingFailure);
        Assert.IsNull(validator.Validate(ticket, wrongPort, out var wrongPortFailure));
        Assert.AreEqual(IdentityTicketFailure.ContextNotAllowed, wrongPortFailure);
    }

    [TestMethod]
    public void InvalidSignatureAndIssuer_ReportDistinctStages()
    {
        using var trustedKey = RSA.Create(2048);
        using var untrustedKey = RSA.Create(2048);
        using var proofKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var validator = CreateValidator(trustedKey, [("localhost", 36676)]);

        byte[] badSignature = CreateTicket(untrustedKey, proofKey, "craftdig-auth", "localhost", 36676);
        Assert.IsNull(validator.Validate(badSignature, out var signatureFailure));
        Assert.AreEqual(IdentityTicketFailure.Signature, signatureFailure);

        byte[] badIssuer = CreateTicket(trustedKey, proofKey, "wrong-issuer", "localhost", 36676);
        Assert.IsNull(validator.Validate(badIssuer, out var issuerFailure));
        Assert.AreEqual(IdentityTicketFailure.Invalid, issuerFailure);
    }

    [TestMethod]
    public void FutureIssuedAtAndOversizedLifetime_AreRejected()
    {
        using var signingKey = RSA.Create(2048);
        using var proofKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var validator = CreateValidator(signingKey, [("localhost", 36676)]);

        byte[] futureTicket = CreateTicket(
            signingKey,
            proofKey,
            "craftdig-auth",
            "localhost",
            36676,
            issuedAt: DateTimeOffset.UtcNow.AddMinutes(5));
        Assert.IsNull(validator.Validate(futureTicket, out var futureFailure));
        Assert.AreEqual(IdentityTicketFailure.Lifetime, futureFailure);

        byte[] longTicket = CreateTicket(
            signingKey,
            proofKey,
            "craftdig-auth",
            "localhost",
            36676,
            lifetime: TimeSpan.FromMinutes(16));
        Assert.IsNull(validator.Validate(longTicket, out var lifetimeFailure));
        Assert.AreEqual(IdentityTicketFailure.Lifetime, lifetimeFailure);
    }

    private static IdentityTicketValidator CreateValidator(
        RSA signingKey,
        (string Host, int Port)[] allowedContexts)
    {
        var log = new AppLog(new AppLogStream());
        var securityKey = new RsaSecurityKey(signingKey.ExportParameters(false)) { KeyId = "test-key" };
        var jwks = new IdentityJwksCache(
            log,
            "https://craftdig.io/.well-known/jwks.json",
            new Dictionary<string, SecurityKey>(StringComparer.Ordinal) { ["test-key"] = securityKey });

        var allowed = new List<ServerContext>();
        foreach ((string host, int port) in allowedContexts)
        {
            Assert.IsTrue(ServerContext.TryParseCanonical(host, port, out var context));
            allowed.Add(context);
        }

        return new(jwks, context => allowed.Contains(context));
    }

    private static byte[] CreateTicket(
        RSA signingKey,
        ECDsa proofKey,
        string issuer,
        string host,
        int port,
        DateTimeOffset? issuedAt = null,
        TimeSpan? lifetime = null)
    {
        var proofParameters = proofKey.ExportParameters(false);
        var now = issuedAt ?? DateTimeOffset.UtcNow.AddSeconds(-1);
        string header = JsonSerializer.Serialize(new
        {
            alg = "RS256",
            kid = "test-key",
            typ = "craftdig-multiplayer-ticket+jwt",
        });
        string claims = JsonSerializer.Serialize(new
        {
            iss = issuer,
            aud = "craftdig:multiplayer:v1",
            sub = Guid.NewGuid().ToString("D"),
            username = "testuser",
            ver = 1,
            sid = Guid.NewGuid().ToString("D"),
            jti = Guid.NewGuid().ToString("D"),
            iat = now.ToUnixTimeSeconds(),
            nbf = now.ToUnixTimeSeconds(),
            exp = now.Add(lifetime ?? TimeSpan.FromMinutes(10)).ToUnixTimeSeconds(),
            server = new { host, port },
            cnf = new
            {
                jwk = new
                {
                    kty = "EC",
                    crv = "P-256",
                    x = Base64UrlEncoder.Encode(proofParameters.Q.X!),
                    y = Base64UrlEncoder.Encode(proofParameters.Q.Y!),
                },
            },
        });

        string encodedHeader = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(header));
        string encodedClaims = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(claims));
        string signingInput = $"{encodedHeader}.{encodedClaims}";
        byte[] signature = signingKey.SignData(
            Encoding.ASCII.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        return Encoding.ASCII.GetBytes($"{signingInput}.{Base64UrlEncoder.Encode(signature)}");
    }
}
