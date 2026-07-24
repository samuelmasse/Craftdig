namespace Craftdig.Client.Security.Test;

[TestClass]
public sealed class PlayerPresenceIdentityRetentionTest
{
    [TestMethod]
    public void ExpiredOffChunkIdentitiesReleaseTheBoundedPlayerCapacity()
    {
        Assert.IsTrue(ServerContext.TryParseCanonical("play.example.com", 36676, out var context));
        using var session = PlayerIdentitySession.CreateAuthenticated(context);
        var log = new Craftdig.App.AppLog(new Craftdig.App.AppLogStream());
        var client = new PlayerPresenceClient(log, null!, session, new PlayerIdentityCache(), null!, null!);
        DateTimeOffset issuedAt = DateTimeOffset.UtcNow;
        const long activityTimestamp = 100;

        for (int i = 1; i <= ProtocolLimits.MaxPresencePlayers; i++)
            client.AddTicketForTest(IdentityTicketTestData.Ticket(session, context, i, issuedAt), activityTimestamp);

        Assert.AreEqual(ProtocolLimits.MaxPresencePlayers, client.StoredPlayerCount);
        client.AddTicketForTest(
            IdentityTicketTestData.Ticket(session, context, ProtocolLimits.MaxPresencePlayers + 1, issuedAt),
            activityTimestamp);
        Assert.AreEqual(ProtocolLimits.MaxPresencePlayers, client.StoredPlayerCount);

        client.PruneInactiveIdentities(activityTimestamp + 31 * System.Diagnostics.Stopwatch.Frequency);
        Assert.AreEqual(0, client.StoredPlayerCount);

        client.AddTicketForTest(
            IdentityTicketTestData.Ticket(session, context, ProtocolLimits.MaxPresencePlayers + 2, issuedAt),
            activityTimestamp);
        Assert.AreEqual(1, client.StoredPlayerCount);
    }
}
