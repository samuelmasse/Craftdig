namespace Craftdig.Client.Security.Test;

[TestClass]
public sealed class PlayerIdentityStatusPolicyTest
{
    [TestMethod]
    public void Evaluate_EnforcesRawInvalidFreshnessAndPendingStates()
    {
        var utcNow = DateTimeOffset.FromUnixTimeSeconds(2_000_000_000);
        DateTimeOffset future = utcNow.AddMinutes(1);

        Assert.AreEqual(PlayerIdentityStatus.Unverified, Evaluate(false, true, 10, 100, utcNow, future, true, true, future));
        Assert.AreEqual(PlayerIdentityStatus.Invalid, Evaluate(true, true, 10, 100, utcNow, future, true, true, future));
        Assert.AreEqual(PlayerIdentityStatus.Verified, Evaluate(true, false, 10, 10, utcNow, future, true, false, future));
        Assert.AreEqual(PlayerIdentityStatus.Stale, Evaluate(true, false, 11, 10, utcNow, future, true, false, future));
        Assert.AreEqual(PlayerIdentityStatus.Stale, Evaluate(true, false, 10, 100, utcNow, utcNow, true, false, future));
        Assert.AreEqual(PlayerIdentityStatus.Stale, Evaluate(true, false, 10, 100, utcNow, future, false, false, future));
        Assert.AreEqual(PlayerIdentityStatus.Stale, Evaluate(true, false, 10, null, utcNow, null, false, true, future));
        Assert.AreEqual(PlayerIdentityStatus.Pending, Evaluate(true, false, 10, null, utcNow, null, false, false, future));
        Assert.AreEqual(PlayerIdentityStatus.Pending, Evaluate(true, false, 10, null, utcNow, null, false, false, null));
        Assert.AreEqual(PlayerIdentityStatus.Unverified, Evaluate(
            true, false, 10, null, utcNow, null, false, false, null, identityExpected: false));
        Assert.AreEqual(PlayerIdentityStatus.Unverified, Evaluate(
            true, false, 10, null, utcNow, null, false, false, null, entPresent: false));
    }

    private static PlayerIdentityStatus Evaluate(
        bool transport,
        bool invalid,
        long now,
        long? deadline,
        DateTimeOffset utcNow,
        DateTimeOffset? verifiedExpiry,
        bool usable,
        bool proofSeen,
        DateTimeOffset? currentExpiry,
        bool identityExpected = true,
        bool entPresent = true) =>
        PlayerIdentityStatusPolicy.Evaluate(new()
        {
            HasVerifiableTransport = transport,
            IdentityExpected = identityExpected,
            EntPresent = entPresent,
            Invalid = invalid,
            ProofSeen = proofSeen,
            VerifiedRoundUsable = usable,
            MonotonicNow = now,
            UtcNow = utcNow,
            VerifiedDeadline = deadline,
            VerifiedTicketExpiresAt = verifiedExpiry,
            CurrentTicketExpiresAt = currentExpiry,
        });

    [TestMethod]
    public void RosterRetention_KeepsEntsAndRecentEvidenceButExpiresOffChunkAtBoundary()
    {
        long retention = System.Diagnostics.Stopwatch.Frequency * 30L;
        Assert.IsTrue(PlayerPresenceRoster.ShouldRetainRosterEntry(true, null, long.MaxValue));
        Assert.IsFalse(PlayerPresenceRoster.ShouldRetainRosterEntry(false, null, 10));
        Assert.IsTrue(PlayerPresenceRoster.ShouldRetainRosterEntry(false, 100, 100 + retention));
        Assert.IsFalse(PlayerPresenceRoster.ShouldRetainRosterEntry(false, 100, 101 + retention));
    }
}
