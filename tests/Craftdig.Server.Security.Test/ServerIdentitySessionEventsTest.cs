namespace Craftdig.Server.Security.Test;

[TestClass]
public sealed class ServerIdentitySessionEventsTest
{
    [TestMethod]
    public void Constructor_RejectsPlayerCapacityAbovePresenceLimit()
    {
        var config = new ServerConfig
        {
            MaxPlayers = ProtocolLimits.MaxPresencePlayers + 1,
        };

        Assert.ThrowsExactly<InvalidDataException>(() => new ServerIdentitySessionEvents(config));
    }

    [TestMethod]
    public void InitialRegistration_ReservesCapacityAndSessionIdUntilDisconnect()
    {
        var capacityEvents = new ServerIdentitySessionEvents(new() { MaxPlayers = 1 });
        var firstSocket = CreateSocket(1);
        var secondSocket = CreateSocket(2);
        try
        {
            Assert.IsTrue(capacityEvents.TryPublishUnverified(firstSocket, out var first));
            Assert.IsFalse(first.IsActivated);
            Assert.IsFalse(capacityEvents.TryPublishUnverified(secondSocket, out _));

            firstSocket.PresenceConnection = first.Connection;
            capacityEvents.PublishDisconnected(firstSocket);
            Assert.IsTrue(capacityEvents.TryPublishUnverified(secondSocket, out _));
        }
        finally
        {
            firstSocket.Disconnect();
            secondSocket.Disconnect();
        }

        var sessionEvents = new ServerIdentitySessionEvents(new() { MaxPlayers = 2 });
        var duplicateSessionId = SessionId.CreateRandom();
        var identity = CreateIdentity(duplicateSessionId);
        firstSocket = CreateSocket(3);
        secondSocket = CreateSocket(4);
        try
        {
            Assert.IsTrue(sessionEvents.TryPublishIdentity(firstSocket, identity, false, out var first));
            Assert.IsFalse(sessionEvents.TryPublishIdentity(secondSocket, identity, false, out _));

            firstSocket.PresenceConnection = first.Connection;
            sessionEvents.PublishDisconnected(firstSocket);
            Assert.IsTrue(sessionEvents.TryPublishIdentity(secondSocket, identity, false, out _));
        }
        finally
        {
            firstSocket.Disconnect();
            secondSocket.Disconnect();
        }
    }

    [TestMethod]
    public void ProofInbox_AllowsTwoRoundsAndTreatsSameRoundAsIdempotent()
    {
        var events = new ServerIdentitySessionEvents(new() { MaxPlayers = 1 });
        var socket = CreateSocket(1);
        try
        {
            var connection = new ServerPresenceConnection(socket, 1, SessionId.CreateRandom());
            var ticketHash = Hash256.Compute("ticket"u8);
            var first = new PresenceProof(Hash256.Compute("round-one"u8), ticketHash, default);
            var second = new PresenceProof(Hash256.Compute("round-two"u8), ticketHash, default);
            var third = new PresenceProof(Hash256.Compute("round-three"u8), ticketHash, default);

            Assert.IsTrue(events.TryPublishProof(connection, first));
            Assert.IsTrue(events.TryPublishProof(connection, first));
            Assert.AreEqual(1, events.Depths().Proofs);
            Assert.IsTrue(events.TryPublishProof(connection, second));
            Assert.AreEqual(2, events.Depths().Proofs);
            Assert.IsFalse(events.TryPublishProof(connection, third));
            Assert.AreEqual(2, events.Depths().Proofs);
        }
        finally
        {
            socket.Disconnect();
        }
    }

    private static NetSocket CreateSocket(long generation)
    {
        var socket = new NetSocket(new LogRuntime(TextWriter.Null).Log, new TcpClient(), Stream.Null);
        socket.ConnectionGeneration = generation;
        return socket;
    }

    private static ServerIdentitySessionSnapshot CreateIdentity(SessionId sessionId)
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
            sessionId,
            context,
            publicKey,
            "test-key",
            Guid.NewGuid(),
            issuedAt,
            issuedAt,
            issuedAt.AddMinutes(10));
        return new(1, 1, ticket);
    }
}
