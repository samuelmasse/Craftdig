namespace Craftdig;

[TestClass]
public sealed class IdentityPresenceFlowTest
{
    [TestMethod]
    public void TicketKey_AuthenticatesAndProducesViewerVerifiablePresence()
    {
        Assert.IsTrue(ServerContext.TryParseCanonical("play.example.com", 36676, out var context));
        Hash256 contextHash = context.ComputeHash();
        Hash256 ticketHash = Hash256.Compute("exact compact Identity ticket bytes"u8);
        Nonce256 serverNonce = ProtocolTestData.Nonce(
            "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f");

        using var ticketKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var thiefKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        Hash256 authDigest = AuthenticationDigest.Compute(contextHash, serverNonce, ticketHash);
        P256Signature authSignature = P256Signature.SignHash(ticketKey, authDigest);

        Assert.IsTrue(authSignature.VerifyHash(ticketKey, authDigest));
        Assert.IsFalse(authSignature.VerifyHash(thiefKey, authDigest));
        Assert.IsFalse(authSignature.VerifyHash(
            ticketKey,
            AuthenticationDigest.Compute(contextHash, Nonce256.CreateRandom(), ticketHash)));

        SessionId aliceSession = ProtocolTestData.Session("11111111-1111-4111-8111-111111111111");
        SessionId bobSession = ProtocolTestData.Session("22222222-2222-4222-8222-222222222222");
        PresenceChallengeRecord[] round =
        [
            new(aliceSession, 7, ProtocolTestData.Nonce(
                "101112131415161718191a1b1c1d1e1f202122232425262728292a2b2c2d2e2f")),
            new(bobSession, 9, ProtocolTestData.Nonce(
                "202122232425262728292a2b2c2d2e2f303132333435363738393a3b3c3d3e3f")),
        ];
        Array.Sort(round);

        Assert.AreEqual(1, round.Count(record => record.SessionId == bobSession && record.Sequence == 9));
        Assert.IsTrue(PresenceRoundDigest.TryCompute(round, out var roundHash));
        Hash256 proofDigest = PresenceProofDigest.Compute(contextHash, roundHash, ticketHash);
        P256Signature proof = P256Signature.SignHash(ticketKey, proofDigest);

        Assert.IsTrue(proof.VerifyHash(ticketKey, proofDigest));
        Assert.IsFalse(proof.VerifyHash(thiefKey, proofDigest));
        Assert.IsFalse(proof.VerifyHash(
            ticketKey,
            PresenceProofDigest.Compute(contextHash, roundHash, Hash256.Compute("another ticket"u8))));

        Assert.IsTrue(ServerContext.TryParseCanonical("other.example.com", 36676, out var wrongContext));
        Assert.IsFalse(proof.VerifyHash(
            ticketKey,
            PresenceProofDigest.Compute(wrongContext.ComputeHash(), roundHash, ticketHash)));
    }

    [TestMethod]
    public void ViewerChallenge_MustAppearExactlyOnceInCanonicalRound()
    {
        PresenceChallengeRecord[] records = ProtocolTestData.Challenges(2);
        PresenceChallengeRecord bob = records[1];

        Assert.AreEqual(1, records.Count(record => record.SessionId == bob.SessionId));
        Assert.IsTrue(PresenceRoundDigest.TryCompute(records, out _));

        PresenceChallengeRecord[] omitted = [records[0]];
        Assert.AreEqual(0, omitted.Count(record => record.SessionId == bob.SessionId));

        PresenceChallengeRecord[] duplicated = [records[0], bob, bob with { Sequence = bob.Sequence + 1 }];
        Array.Sort(duplicated);
        Assert.AreEqual(2, duplicated.Count(record => record.SessionId == bob.SessionId));
        Assert.IsFalse(PresenceRoundDigest.TryCompute(duplicated, out _));
    }
}
