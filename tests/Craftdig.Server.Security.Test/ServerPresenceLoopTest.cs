namespace Craftdig;

[TestClass]
public sealed class ServerPresenceLoopTest
{
    [TestMethod]
    public void Stop_CancelsRegistrationPendingActivation()
    {
        var log = new LogRuntime(TextWriter.Null).Log;
        var config = new ServerConfig { MaxPlayers = 1 };
        var events = new ServerIdentitySessionEvents(config);
        var metrics = new ServerPresenceMetrics();
        var loop = new ServerPresenceLoop(log, config, events, metrics);
        using var pair = ConnectedPair.Create();
        pair.Server.ConnectionGeneration = 1;

        Assert.IsTrue(events.TryPublishIdentity(
            pair.Server,
            CreateIdentity(pair.Server.ConnectionGeneration),
            false,
            out var registration));
        pair.Server.PresenceConnection = registration.Connection;

        loop.Start();
        loop.Stop();
        loop.Join();

        Assert.IsTrue(registration.Connection.IsCanceled);
        Assert.IsFalse(pair.Server.Connected);
        Assert.AreEqual(0, metrics.Snapshot().ActiveSessions);
    }

    [TestMethod]
    public void FullOutputRing_DisconnectsPresenceSessionAfterThreeBackpressureStrikes()
    {
        var log = new LogRuntime(TextWriter.Null).Log;
        var config = new ServerConfig { MaxPlayers = 1 };
        var events = new ServerIdentitySessionEvents(config);
        var metrics = new ServerPresenceMetrics();
        var loop = new ServerPresenceLoop(log, config, events, metrics);
        using var pair = ConnectedPair.Create();
        pair.Server.ConnectionGeneration = 1;

        var maximumFrameBody = new byte[ProtocolLimits.MaxMessageSize];
        for (int i = 0; i < 4; i++)
            Assert.IsTrue(pair.Server.TrySend(1, maximumFrameBody, []));
        Assert.IsFalse(pair.Server.TrySend(1, [], []));

        var identity = CreateIdentity(pair.Server.ConnectionGeneration);
        Assert.IsTrue(events.TryPublishIdentity(pair.Server, identity, false, out var registration));
        pair.Server.PresenceConnection = registration.Connection;
        registration.Activate();

        loop.Start();
        try
        {
            Assert.IsTrue(
                SpinWait.SpinUntil(
                    () => registration.Connection.IsCanceled &&
                        metrics.Snapshot().SlowSocketDisconnects == 1,
                    TimeSpan.FromSeconds(2)),
                $"Presence loop did not disconnect the backpressured socket: {metrics.Snapshot()}");

            var snapshot = metrics.Snapshot();
            Assert.AreEqual(3, snapshot.BackpressureEvents);
            Assert.AreEqual(1, snapshot.SlowSocketDisconnects);
            Assert.IsTrue(registration.Connection.IsCanceled);
            Assert.IsFalse(pair.Server.Connected);
        }
        finally
        {
            loop.Stop();
            loop.Join();
        }
    }

    private static ServerIdentitySessionSnapshot CreateIdentity(long connectionGeneration)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var parameters = key.ExportParameters(false);
        Assert.IsTrue(P256PublicKey.TryCreate(parameters.Q.X!, parameters.Q.Y!, out var publicKey));
        Assert.IsTrue(ServerContext.TryCreate("localhost", 36676, out var context));
        var issuedAt = DateTimeOffset.UtcNow;
        var ticket = new ValidatedIdentityTicket(
            "a.b.c"u8,
            Guid.NewGuid(),
            "test",
            SessionId.CreateRandom(),
            context,
            publicKey,
            "test-key",
            Guid.NewGuid(),
            issuedAt,
            issuedAt,
            issuedAt.AddMinutes(10));
        return new(connectionGeneration, 1, ticket);
    }
}
