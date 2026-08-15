namespace Craftdig;

[TestClass]
public sealed class PlayerPresenceSigningGuardTest
{
    [TestMethod]
    public void Guard_RejectsReplayAndCapsRecentRoundsUntilOriginalDeadlineExpires()
    {
        var guard = new PlayerPresenceSigningGuard(2);
        Hash256 first = ClientTestData.Hash(1);
        Hash256 second = ClientTestData.Hash(2);
        Hash256 third = ClientTestData.Hash(3);

        Assert.IsTrue(guard.TryRecord(first, 100));
        Assert.IsFalse(guard.TryRecord(first, 500));
        Assert.IsTrue(guard.TryRecord(second, 200));
        Assert.IsFalse(guard.HasCapacity);
        Assert.IsFalse(guard.TryRecord(third, 300));

        guard.Expire(100);
        Assert.IsTrue(guard.Contains(first));
        guard.Expire(101);
        Assert.IsFalse(guard.Contains(first));
        Assert.IsTrue(guard.Contains(second));
        Assert.IsTrue(guard.TryRecord(third, 300));
        Assert.AreEqual(2, guard.Count);

        guard.Clear();
        Assert.AreEqual(0, guard.Count);
    }
}
