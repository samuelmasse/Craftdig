namespace Craftdig.Protocol.Security.Test;

[TestClass]
public sealed class PresenceSecurityTest
{
    [TestMethod]
    public void ValidatedTicket_RequiresTimeAfterNotBefore()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var parameters = key.ExportParameters(false);
        Assert.IsTrue(P256PublicKey.TryCreate(parameters.Q.X!, parameters.Q.Y!, out var publicKey));
        Assert.IsTrue(ServerContext.TryCreate("localhost", 36676, out var context));
        var issuedAt = DateTimeOffset.UtcNow;
        var notBefore = issuedAt.AddMinutes(1);

        Assert.ThrowsExactly<ArgumentException>(() => new ValidatedIdentityTicket(
            "a.b.c"u8,
            Guid.NewGuid(),
            "test",
            SessionId.CreateRandom(),
            context,
            publicKey,
            "test-key",
            Guid.NewGuid(),
            issuedAt,
            notBefore,
            notBefore));
    }

    [TestMethod]
    public void P256Signature_SignHash_VerifiesOnlyMatchingKeyAndDigest()
    {
        using var signer = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var wrongKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var digest = Hash256.Compute("Craftdig deterministic digest fixture"u8);
        var wrongDigest = Hash256.Compute("Craftdig different digest fixture"u8);

        P256Signature signature = P256Signature.SignHash(signer, digest);
        var encoded = new byte[P256Signature.Size];
        Assert.IsTrue(signature.TryWrite(encoded));
        Assert.AreEqual(64, encoded.Length);
        Assert.IsTrue(P256Signature.TryRead(encoded, out var decoded));
        Assert.AreEqual(signature, decoded);
        Assert.IsTrue(decoded.VerifyHash(signer, digest));
        Assert.IsFalse(decoded.VerifyHash(wrongKey, digest));
        Assert.IsFalse(decoded.VerifyHash(signer, wrongDigest));

        ECParameters signerParameters = signer.ExportParameters(false);
        Assert.IsTrue(P256PublicKey.TryCreate(signerParameters.Q.X!, signerParameters.Q.Y!, out var publicKey));
        using var publicVerifier = publicKey.CreateEcdsa();
        Assert.IsTrue(decoded.VerifyHash(publicVerifier, digest));
        Assert.IsFalse(P256PublicKey.TryCreate(new byte[32], new byte[32], out _));
    }

    [TestMethod]
    public void PresenceRoundDigest_RequiresCanonicalUniqueSessions()
    {
        PresenceChallengeRecord[] sorted = ProtocolTestData.Challenges(3);
        Assert.IsTrue(PresenceRoundDigest.TryCompute(sorted, out _));
        Assert.IsTrue(PresenceRoundDigest.TryCompute(sorted.AsSpan(0, 1), out _));
        Assert.IsFalse(PresenceRoundDigest.TryCompute([], out _));

        PresenceChallengeRecord[] reversed = [.. sorted.Reverse()];
        Assert.IsFalse(PresenceRoundDigest.TryCompute(reversed, out _));

        PresenceChallengeRecord[] exactDuplicate = [sorted[0], sorted[0]];
        Assert.IsFalse(PresenceRoundDigest.TryCompute(exactDuplicate, out _));

        PresenceChallengeRecord[] duplicateSession = [sorted[0], sorted[0] with { Sequence = 1 }];
        Assert.IsTrue(duplicateSession[0].CompareTo(duplicateSession[1]) < 0);
        Assert.IsFalse(PresenceRoundDigest.TryCompute(duplicateSession, out _));
    }

    [TestMethod]
    public void PresenceRoundDigest_EnforcesPlayerCountCap()
    {
        PresenceChallengeRecord[] maximum = ProtocolTestData.Challenges(ProtocolLimits.MaxPresencePlayers);
        Assert.IsTrue(PresenceRoundDigest.TryCompute(maximum, out _));

        PresenceChallengeRecord[] oversized = ProtocolTestData.Challenges(ProtocolLimits.MaxPresencePlayers + 1);
        Assert.IsFalse(PresenceRoundDigest.TryCompute(oversized, out _));
    }

    [TestMethod]
    public void PresenceChallengeRecord_RejectsInvalidSessionEncoding()
    {
        PresenceChallengeRecord record = ProtocolTestData.Challenges(1)[0];
        var bytes = new byte[PresenceChallengeRecord.Size];
        Assert.IsTrue(record.TryWrite(bytes));
        bytes[6] = 0x50;
        Assert.IsFalse(PresenceChallengeRecord.TryRead(bytes, out _));

        Assert.IsTrue(record.TryWrite(bytes));
        bytes[8] = 0x00;
        Assert.IsFalse(PresenceChallengeRecord.TryRead(bytes, out _));
    }
}
