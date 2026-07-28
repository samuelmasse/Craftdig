namespace Craftdig.Server.Security.Test;

[TestClass]
public sealed class ServerPresencePerformanceTest
{
    private const int AverageTicketBytes = 1024;
    private const int RefreshIntervalSeconds = 10 * 60;

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [DataRow(20, 3_162L, 63_240L, 7_010L)]
    [DataRow(100, 15_512L, 1_551_200L, 172_286L)]
    [DataRow(1_000, 154_486L, 154_486_000L, 17_165_266L)]
    public void RoundEncoding_StaysWithinAllocationAndEgressBudgets(
        int playerCount,
        long expectedPerClientBytes,
        long expectedEgressPerRound,
        long expectedCombinedBytesPerSecond)
    {
        var records = Challenges(playerCount);
        var proof = CreateProof();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();

        var round = new ServerPresenceRound(1, Stopwatch.GetTimestamp(), records);
        for (int i = 0; i < playerCount; i++)
            round.AddProof(proof, 1);
        round.FlushProofs();

        stopwatch.Stop();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        long roundChunkBytes = round.Chunks.Sum(body => body.Length + ProtocolLimits.FrameHeaderSize);
        long proofBatchBytes = round.ProofBatches.Sum(body => body.Length + ProtocolLimits.FrameHeaderSize);
        long perClientBytes = roundChunkBytes + proofBatchBytes;
        long egressPerRound = perClientBytes * playerCount;
        long refreshBytesPerSecond = (long)playerCount * playerCount *
            (AverageTicketBytes + ProtocolLimits.FrameHeaderSize) / RefreshIntervalSeconds;
        long combinedBytesPerSecond = egressPerRound / 10 + refreshBytesPerSecond;

        TestContext.WriteLine(
            $"N={playerCount}: encodeMs={stopwatch.Elapsed.TotalMilliseconds:F3}, " +
            $"allocated={allocated}, perClient={perClientBytes}, round={egressPerRound}, " +
            $"combinedPerSecond={combinedBytesPerSecond}");

        Assert.AreEqual(expectedPerClientBytes, perClientBytes);
        Assert.AreEqual(expectedEgressPerRound, egressPerRound);
        Assert.AreEqual(expectedCombinedBytesPerSecond, combinedBytesPerSecond);
        Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(1),
            $"Round encoding took {stopwatch.Elapsed.TotalMilliseconds:F3} ms.");
        Assert.IsTrue(allocated < 1_000_000L, $"Round encoding allocated {allocated} bytes.");
        if (playerCount == ProtocolLimits.MaxPresencePlayers)
            Assert.IsTrue(combinedBytesPerSecond < 20_000_000L);
    }

    [TestMethod]
    public void P256Verification_SustainsThePresenceInputRate()
    {
        const int verificationCount = ProtocolLimits.MaxPresencePlayers;
        var digest = Hash256.Compute("presence-performance"u8);
        using var privateKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var publicKey = ECDsa.Create(privateKey.ExportParameters(false));
        var signature = P256Signature.SignHash(privateKey, digest);

        Assert.IsTrue(signature.VerifyHash(publicKey, digest));
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < verificationCount; i++)
            Assert.IsTrue(signature.VerifyHash(publicKey, digest));
        stopwatch.Stop();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        double verificationsPerSecond = verificationCount / stopwatch.Elapsed.TotalSeconds;

        TestContext.WriteLine(
            $"P-256: count={verificationCount}, elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F3}, " +
            $"perSecond={verificationsPerSecond:F0}, allocated={allocated}");

        Assert.IsTrue(verificationsPerSecond >= 150d,
            $"P-256 verification reached only {verificationsPerSecond:F0} operations/s.");
        Assert.IsTrue(allocated < 8_000_000L, $"P-256 verification allocated {allocated} bytes.");
    }

    [TestMethod]
    public void ThousandPlayerInboxBurst_RemainsBounded()
    {
        const int playerCount = ProtocolLimits.MaxPresencePlayers;
        var events = new ServerIdentitySessionEvents(new() { MaxPlayers = playerCount });
        var socket = new NetSocket(new LogRuntime(TextWriter.Null).Log, new TcpClient(), Stream.Null);
        var records = Challenges(playerCount);
        var connections = records
            .Select(record => new ServerPresenceConnection(socket, 1, record.SessionId))
            .ToArray();
        var firstRound = Hash256.Compute("round-one"u8);
        var secondRound = Hash256.Compute("round-two"u8);
        var ticketHash = Hash256.Compute("ticket"u8);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < playerCount; i++)
        {
            Assert.IsTrue(events.TryPublishChallenge(
                connections[i],
                new((ulong)i, records[i].Nonce)));
            Assert.IsTrue(events.TryPublishProof(
                connections[i],
                new(firstRound, ticketHash, default)));
            Assert.IsTrue(events.TryPublishProof(
                connections[i],
                new(secondRound, ticketHash, default)));
        }

        stopwatch.Stop();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        var depths = events.Depths();
        var overflow = new ServerPresenceConnection(socket, 1, SessionId.CreateRandom());

        TestContext.WriteLine(
            $"Inbox N={playerCount}: elapsedMs={stopwatch.Elapsed.TotalMilliseconds:F3}, " +
            $"allocated={allocated}, challenges={depths.Challenges}, proofs={depths.Proofs}");

        Assert.AreEqual(playerCount, depths.Challenges);
        Assert.AreEqual(playerCount * ProtocolLimits.MaxPresenceActiveRounds, depths.Proofs);
        Assert.IsFalse(events.TryPublishChallenge(overflow, new(1, Nonce256.CreateRandom())));
        Assert.IsFalse(events.TryPublishProof(
            overflow,
            new(firstRound, ticketHash, default)));
        Assert.IsFalse(events.TryPublishProof(
            connections[0],
            new(Hash256.Compute("third-round"u8), ticketHash, default)));
        Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(1),
            $"Inbox burst took {stopwatch.Elapsed.TotalMilliseconds:F3} ms.");
        Assert.IsTrue(allocated < 8_000_000L, $"Inbox burst allocated {allocated} bytes.");
        socket.Disconnect();
    }

    private static PresenceProofRecord CreateProof()
    {
        Span<byte> signatureBytes = stackalloc byte[P256Signature.Size];
        signatureBytes.Fill(1);
        if (!P256Signature.TryRead(signatureBytes, out var signature))
            throw new InvalidOperationException("The fixed-width proof fixture is invalid.");

        return new(Hash256.Compute("ticket"u8), signature);
    }

    private static PresenceChallengeRecord[] Challenges(int count)
    {
        var records = new PresenceChallengeRecord[count];
        Span<byte> sessionBytes = stackalloc byte[SessionId.Size];
        Span<byte> nonceBytes = stackalloc byte[Nonce256.Size];
        for (int i = 0; i < count; i++)
        {
            sessionBytes.Clear();
            sessionBytes[6] = 0x40;
            sessionBytes[8] = 0x80;
            BinaryPrimitives.WriteUInt32BigEndian(sessionBytes[12..], (uint)i);
            if (!SessionId.TryRead(sessionBytes, out var sessionId))
                throw new InvalidOperationException("The deterministic session fixture is invalid.");

            nonceBytes.Clear();
            BinaryPrimitives.WriteUInt32BigEndian(nonceBytes[28..], (uint)i);
            if (!Nonce256.TryRead(nonceBytes, out var nonce))
                throw new InvalidOperationException("The deterministic nonce fixture is invalid.");

            records[i] = new(sessionId, (ulong)i, nonce);
        }

        return records;
    }
}
