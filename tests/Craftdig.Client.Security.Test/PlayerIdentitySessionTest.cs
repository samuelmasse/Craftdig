namespace Craftdig;

[TestClass]
public sealed class PlayerIdentitySessionTest
{
    [TestMethod]
    public void Session_CanBeTransferredIntoThePlayerScope()
    {
        Assert.IsTrue(ServerContext.TryParseCanonical("play.example.com", 36676, out var context));
        using var session = PlayerIdentitySession.CreateAuthenticated(context);
        var playerScope = new Injector().Scope<PlayerScope>().With(session);

        Assert.AreSame(session, playerScope.Get<PlayerIdentitySession>());
    }

    [TestMethod]
    public void Session_SignsAdmissionAndPresenceForItsBoundCurrentTicket()
    {
        Assert.IsTrue(ServerContext.TryParseCanonical("play.example.com", 36676, out var context));
        using var session = PlayerIdentitySession.CreateAuthenticated(context);
        DateTimeOffset issuedAt = DateTimeOffset.UtcNow;
        var ticket = IdentityTicketTestData.Ticket(session, context, 1, issuedAt);
        session.InstallTicket(ticket);

        var nonce = ClientTestData.Challenges(1)[0].Nonce;
        var authentication = session.SignAuthentication(ticket, nonce);
        using var verifier = session.PublicKey.CreateEcdsa();
        Assert.IsTrue(authentication.VerifyHash(
            verifier,
            AuthenticationDigest.Compute(context.ComputeHash(), nonce, ticket.TicketHash)));
        Assert.IsFalse(authentication.VerifyHash(
            verifier,
            AuthenticationDigest.Compute(context.ComputeHash(), Nonce256.CreateRandom(), ticket.TicketHash)));

        Hash256 roundHash = ClientTestData.Hash(10);
        Assert.IsTrue(session.TrySignPresence(roundHash, ticket.TicketHash, out var presence));
        Assert.IsTrue(presence.VerifyHash(
            verifier,
            PresenceProofDigest.Compute(context.ComputeHash(), roundHash, ticket.TicketHash)));
        Assert.IsFalse(session.TrySignPresence(roundHash, ClientTestData.Hash(11), out _));
    }

    [TestMethod]
    public void Refresh_AtomicallyReplacesTheTicketAcceptedForPresenceSigning()
    {
        Assert.IsTrue(ServerContext.TryParseCanonical("play.example.com", 36676, out var context));
        using var session = PlayerIdentitySession.CreateAuthenticated(context);
        Guid playerId = Guid.NewGuid();
        DateTimeOffset issuedAt = DateTimeOffset.UtcNow;
        var original = IdentityTicketTestData.Ticket(session, context, 1, issuedAt, playerId);
        var refreshed = IdentityTicketTestData.Ticket(session, context, 2, issuedAt.AddMinutes(1), playerId);

        session.InstallTicket(original);
        session.InstallTicket(refreshed);

        Hash256 roundHash = ClientTestData.Hash(20);
        Assert.AreSame(refreshed, session.GetCurrentTicket());
        Assert.IsFalse(session.TrySignPresence(roundHash, original.TicketHash, out _));
        Assert.IsTrue(session.TrySignPresence(roundHash, refreshed.TicketHash, out _));
    }

    [TestMethod]
    public void Reconnect_UsesASeparateSessionAndDisposedKeysCannotSign()
    {
        Assert.IsTrue(ServerContext.TryParseCanonical("play.example.com", 36676, out var context));
        var disconnected = PlayerIdentitySession.CreateAuthenticated(context);
        using var reconnected = PlayerIdentitySession.CreateAuthenticated(context);
        var oldTicket = IdentityTicketTestData.Ticket(disconnected, context, 1, DateTimeOffset.UtcNow);
        disconnected.InstallTicket(oldTicket);

        Assert.AreNotEqual(disconnected.SessionId, reconnected.SessionId);
        Assert.AreNotEqual(disconnected.PublicKey, reconnected.PublicKey);

        disconnected.Dispose();
        Assert.IsFalse(disconnected.TrySignPresence(ClientTestData.Hash(30), oldTicket.TicketHash, out _));
        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            disconnected.SignAuthentication(oldTicket, Nonce256.CreateRandom()));
    }
}
