namespace Craftdig.Identity.Test;

[TestClass]
public sealed class DevIdentityTicketIssuerTest
{
    [TestMethod]
    public void IssuedDevTicket_ValidatesAgainstTheSeededDevKey()
    {
        string keyPath = Path.Combine(Path.GetTempPath(), $"craftdig-dev-key-{Guid.NewGuid():N}.pkcs8");
        try
        {
            var key = DevIdentityKey.LoadOrCreate(keyPath);
            using var proofKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var proofParameters = proofKey.ExportParameters(false);
            Assert.IsTrue(P256PublicKey.TryCreate(proofParameters.Q.X!, proofParameters.Q.Y!, out var publicKey));
            Assert.IsTrue(ServerContext.TryParseCanonical("127.0.0.1", 36676, out var context));
            var sessionId = SessionId.CreateRandom();
            Guid playerId = DevIdentityKey.PlayerId("alice");

            byte[] rawTicket = new DevIdentityTicketIssuer(key).Issue(
                playerId,
                "alice",
                sessionId,
                context,
                publicKey,
                DateTimeOffset.UtcNow.AddSeconds(-1),
                TimeSpan.FromMinutes(10));

            var log = new LogRuntime(TextWriter.Null).Log;
            var jwks = IdentityJwksCache.Seeded(log, key.PublicKey);
            var validator = new IdentityTicketValidator(jwks, ctx => ctx == context);

            var validated = validator.Validate(rawTicket, context, out var failure);

            Assert.IsNotNull(validated);
            Assert.AreEqual(IdentityTicketFailure.None, failure);
            Assert.AreEqual(playerId, validated.PlayerId);
            Assert.AreEqual("alice", validated.Username);
            Assert.AreEqual(sessionId, validated.SessionId);
            Assert.AreEqual(publicKey, validated.PublicKey);
        }
        finally
        {
            File.Delete(keyPath);
        }
    }

    [TestMethod]
    public void PlayerId_IsStablePerName_AndDistinctAcrossNames()
    {
        Assert.AreEqual(DevIdentityKey.PlayerId("alice"), DevIdentityKey.PlayerId("alice"));
        Assert.AreNotEqual(DevIdentityKey.PlayerId("alice"), DevIdentityKey.PlayerId("bob"));
        Assert.AreNotEqual(Guid.Empty, DevIdentityKey.PlayerId("alice"));
    }
}
