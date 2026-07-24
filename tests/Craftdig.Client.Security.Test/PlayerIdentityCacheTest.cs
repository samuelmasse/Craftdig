namespace Craftdig.Client.Security.Test;

[TestClass]
public sealed class PlayerIdentityCacheTest
{
    [TestMethod]
    public void Publish_CopiesToAnImmutableSnapshot()
    {
        var cache = new PlayerIdentityCache();
        Guid playerId = Guid.NewGuid();
        var snapshot = new PlayerIdentitySnapshot(
            playerId,
            "Alice",
            PlayerIdentityStatus.Verified,
            true);
        var source = new Dictionary<Guid, PlayerIdentitySnapshot> { [playerId] = snapshot };

        cache.Publish(source);
        source.Clear();

        Assert.AreEqual(1, cache.Players.Count);
        Assert.AreSame(snapshot, cache.Players[playerId]);
        Assert.IsFalse(cache.Players is Dictionary<Guid, PlayerIdentitySnapshot>);
        if (cache.Players is IDictionary<Guid, PlayerIdentitySnapshot> mutableView)
        {
            Assert.IsTrue(mutableView.IsReadOnly);
            Assert.ThrowsExactly<NotSupportedException>(() => mutableView.Clear());
        }
    }

    [TestMethod]
    public void EntReconciliation_IsKeyedOnlyByExactPlayerId()
    {
        var cache = new PlayerIdentityCache();
        Guid ticketPlayerId = Guid.NewGuid();
        Guid wrongEntId = Guid.NewGuid();

        cache.ObservePlayerEnt(wrongEntId);

        Assert.IsTrue(cache.IsEntPresent(wrongEntId));
        Assert.IsFalse(cache.IsEntPresent(ticketPlayerId));
        CollectionAssert.AreEquivalent(new[] { wrongEntId }, cache.CaptureEnts());
    }

    [TestMethod]
    public void EntReconciliation_IsBoundedToTheProtocolPlayerLimit()
    {
        var cache = new PlayerIdentityCache();
        Span<byte> bytes = stackalloc byte[16];
        for (int i = 0; i <= ProtocolLimits.MaxPresencePlayers; i++)
        {
            bytes.Clear();
            BinaryPrimitives.WriteInt32BigEndian(bytes[12..], i + 1);
            cache.ObservePlayerEnt(new Guid(bytes));
        }

        Assert.AreEqual(ProtocolLimits.MaxPresencePlayers, cache.CaptureEnts().Length);
    }
}
