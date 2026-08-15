namespace Craftdig;

[TestClass]
public sealed class ServerAuthTransportTest
{
    [TestMethod]
    public async Task AuthenticationSuccess_UsesAnExactlyEmptyResultBody()
    {
        var noAuth = CreateNoAuth();
        using var pair = ConnectedPair.Create();
        using var cancellation = new CancellationTokenSource();
        using var readTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var push = Task.Run(() => pair.Server.Push(cancellation.Token));
        try
        {
            noAuth.Authenticate(pair.Server, "test"u8);

            var header = new byte[ProtocolLimits.FrameHeaderSize];
            await pair.ClientStream.ReadExactlyAsync(header, readTimeout.Token);
            Assert.AreEqual(ResultAuthCommand.CommandId, BinaryPrimitives.ReadUInt16BigEndian(header));
            Assert.AreEqual(0, BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(sizeof(ushort))));
        }
        finally
        {
            cancellation.Cancel();
            await push.WaitAsync(TimeSpan.FromSeconds(1));
        }
    }

    [TestMethod]
    public void BeginAuthentication_RequiresAnExactlyEmptyBody()
    {
        var log = new LogRuntime(TextWriter.Null).Log;
        var receiver = new ServerAuthReceiver(log, CreateAuth(), CreateNoAuth());

        using var malformed = ConnectedPair.Create();
        malformed.Server.IsTransportSecure = true;
        receiver.BeginAuth(malformed.Server, [0]);
        Assert.IsFalse(malformed.Server.Connected);
        Assert.IsNull(malformed.Server.AuthChallenge);

        using var exact = ConnectedPair.Create();
        exact.Server.IsTransportSecure = true;
        receiver.BeginAuth(exact.Server, []);
        Assert.IsTrue(exact.Server.Connected);
        Assert.IsNotNull(exact.Server.AuthChallenge);
    }

    [TestMethod]
    public void IdentityAuthentication_OnRawTransport_IsRejectedBeforeChallengeOrTicketParsing()
    {
        var auth = CreateAuth();

        using var beginPair = ConnectedPair.Create();
        auth.SendNonce(beginPair.Server);
        Assert.IsNull(beginPair.Server.AuthChallenge);
        Assert.IsFalse(beginPair.Server.IsAuthenticated);

        using var completePair = ConnectedPair.Create();
        auth.AuthenticateTicket(completePair.Server, []);
        Assert.IsNull(completePair.Server.AuthChallenge);
        Assert.IsFalse(completePair.Server.IsAuthenticated);
    }

    [TestMethod]
    public void NoAuthName_WithControlCharacters_IsRejected()
    {
        var noAuth = CreateNoAuth();
        using var pair = ConnectedPair.Create();

        noAuth.Authenticate(pair.Server, "bad\nname"u8);

        Assert.IsFalse(pair.Server.IsAuthenticated);
        Assert.IsNull(pair.Server.PresenceConnection);
    }

    [TestMethod]
    public void ExpiryCheck_IsRevisionAware_AndNoAuthRemainsAllowed()
    {
        var auth = CreateAuth();
        using var pair = ConnectedPair.Create();
        var now = DateTimeOffset.UtcNow;
        var expired = new ServerIdentitySessionSnapshot(1, 1, CreateTicket(now.AddSeconds(-1)));
        var refreshed = new ServerIdentitySessionSnapshot(1, 2, CreateTicket(now.AddMinutes(10)));

        pair.Server.IdentitySession = refreshed;
        pair.Server.IsAuthenticated = true;
        Assert.IsFalse(auth.ExpireIdentitySession(pair.Server, expired, now));
        Assert.IsTrue(auth.AuthorizeGameplay(pair.Server));

        pair.Server.IdentitySession = expired;
        Assert.IsFalse(auth.AuthorizeGameplay(pair.Server));
        Assert.IsFalse(pair.Server.IsAuthenticated);

        using var noAuthPair = ConnectedPair.Create();
        noAuthPair.Server.IsAuthenticated = true;
        Assert.IsTrue(auth.AuthorizeGameplay(noAuthPair.Server));
    }

    [TestMethod]
    public void AuthenticationResultBackpressure_RollsBackAndCancelsPresence()
    {
        var noAuth = CreateNoAuth();
        using var pair = ConnectedPair.Create();
        var body = new byte[ProtocolLimits.MaxMessageSize];
        for (int i = 0; i < 4; i++)
            Assert.IsTrue(pair.Server.TrySend(1, body, []));

        noAuth.Authenticate(pair.Server, "test"u8);

        Assert.IsFalse(pair.Server.IsAuthenticated);
        Assert.IsNull(pair.Server.AuthenticatedUid);
        Assert.IsTrue(pair.Server.PresenceConnection?.IsCanceled);
    }

    [TestMethod]
    public void PrivateAllowlist_UsesPlayerIdOnly()
    {
        Guid allowedPlayerId = Guid.NewGuid();
        string root = Path.Combine(Path.GetTempPath(), $"craftdig-allowlist-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var allowlist = new SeverAllowlist(
                new() { Allowlist = [allowedPlayerId.ToString("D"), "Alice"] },
                new() { RootPath = root });

            Assert.IsTrue(allowlist.Allow(allowedPlayerId));
            Assert.IsFalse(allowlist.Allow(Guid.NewGuid()));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static ValidatedIdentityTicket CreateTicket(DateTimeOffset expiresAt)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var parameters = key.ExportParameters(false);
        Assert.IsTrue(P256PublicKey.TryCreate(parameters.Q.X!, parameters.Q.Y!, out var publicKey));
        Assert.IsTrue(ServerContext.TryCreate("localhost", 36676, out var context));
        var issuedAt = expiresAt.AddMinutes(-10);
        Assert.IsTrue(SessionId.TryFromGuid(Guid.NewGuid(), out var sessionId));
        return new(
            "a.b.c"u8,
            Guid.NewGuid(),
            "test",
            sessionId,
            context,
            publicKey,
            "test-key",
            Guid.NewGuid(),
            issuedAt,
            issuedAt,
            expiresAt);
    }

    private static ServerAuth CreateAuth()
    {
        var log = new LogRuntime(TextWriter.Null).Log;
        var config = new ServerConfig
        {
            PublicServer = true,
        };
        var defaults = new ServerDefaults();
        var sockets = new ServerSockets();
        var limits = new ServerClientLimits(log, sockets);
        var allowlist = new SeverAllowlist(defaults, config);
        var guards = new ServerAuthGuards(log, config, sockets, allowlist);
        var contexts = new ServerPublicContexts(log, config);
        var validator = new ServerIdentityTrust(log, config, contexts, new());
        var events = new ServerIdentitySessionEvents(config);
        return new(log, limits, guards, validator, events);
    }

    private static ServerNoAuth CreateNoAuth()
    {
        var log = new LogRuntime(TextWriter.Null).Log;
        var config = new ServerConfig { PublicServer = true };
        var sockets = new ServerSockets();
        var limits = new ServerClientLimits(log, sockets);
        var guards = new ServerAuthGuards(log, config, sockets, new(new(), config));
        return new(log, limits, guards, new(config));
    }

}
