namespace Craftdig;

[TestClass]
public sealed class PresenceBudgetTest
{
    [TestMethod]
    [DataRow(20, 3_040L, 60_800L, 6_080L, 2_240L, 3_840L, 686L, 6_766L)]
    [DataRow(100, 15_200L, 1_520_000L, 152_000L, 56_000L, 96_000L, 17_166L, 169_166L)]
    [DataRow(1_000, 152_000L, 152_000_000L, 15_200_000L, 5_600_000L, 9_600_000L, 1_716_666L, 16_916_666L)]
    public void PresencePayloadSimulation_MatchesDocumentedBudget(
        int playerCount,
        long expectedPerClient,
        long expectedServerEgressPerRound,
        long expectedServerEgressPerSecond,
        long expectedChallengeEgressPerSecond,
        long expectedProofEgressPerSecond,
        long expectedRefreshEgressPerSecond,
        long expectedCombinedEgressPerSecond)
    {
        PresenceChallengeRecord[] challenges = ProtocolTestData.Challenges(playerCount);
        Assert.IsTrue(PresenceRoundDigest.TryCompute(challenges, out var roundHash));
        PresenceProofRecord[] proofs = ProtocolTestData.Proofs(playerCount);

        int chunkCount = DivideRoundUp(playerCount, PresenceRoundChunkCommandCodec.MaxRecordCount);
        int encodedChallengeBytes = 0;
        int challengeRecordsRead = 0;
        for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
        {
            int recordOffset = chunkIndex * PresenceRoundChunkCommandCodec.MaxRecordCount;
            int recordCount = Math.Min(
                PresenceRoundChunkCommandCodec.MaxRecordCount,
                playerCount - recordOffset);
            var header = new PresenceRoundChunkHeader(
                roundHash,
                (ushort)chunkIndex,
                (ushort)chunkCount,
                (uint)playerCount);
            var buffer = new byte[PresenceRoundChunkCommandCodec.MaxSize];
            Assert.IsTrue(PresenceRoundChunkCommandCodec.TryWrite(
                buffer,
                header,
                challenges.AsSpan(recordOffset, recordCount),
                out int written));
            Assert.IsTrue(PresenceRoundChunkCommandCodec.TryRead(
                buffer.AsSpan(0, written),
                out var decodedHeader,
                out var recordBytes));
            Assert.AreEqual(header, decodedHeader);
            challengeRecordsRead += recordBytes.Length / PresenceChallengeRecord.Size;
            encodedChallengeBytes += written;
        }

        int batchCount = DivideRoundUp(playerCount, PresenceProofBatchCommandCodec.MaxRecordCount);
        int encodedProofBytes = 0;
        int proofRecordsRead = 0;
        for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
        {
            int recordOffset = batchIndex * PresenceProofBatchCommandCodec.MaxRecordCount;
            int recordCount = Math.Min(
                PresenceProofBatchCommandCodec.MaxRecordCount,
                playerCount - recordOffset);
            var buffer = new byte[PresenceProofBatchCommandCodec.MaxSize];
            Assert.IsTrue(PresenceProofBatchCommandCodec.TryWrite(
                buffer,
                roundHash,
                proofs.AsSpan(recordOffset, recordCount),
                out int written));
            Assert.IsTrue(PresenceProofBatchCommandCodec.TryRead(
                buffer.AsSpan(0, written),
                out var decodedRoundHash,
                out var recordBytes));
            Assert.AreEqual(roundHash, decodedRoundHash);
            proofRecordsRead += recordBytes.Length / PresenceProofRecord.Size;
            encodedProofBytes += written;
        }

        Assert.AreEqual(playerCount, challengeRecordsRead);
        Assert.AreEqual(playerCount, proofRecordsRead);
        Assert.AreEqual(
            playerCount * PresenceChallengeRecord.Size + chunkCount * PresenceRoundChunkCommandCodec.HeaderSize,
            encodedChallengeBytes);
        Assert.AreEqual(
            playerCount * PresenceProofRecord.Size + batchCount * PresenceProofBatchCommandCodec.HeaderSize,
            encodedProofBytes);

        long perClient = (long)playerCount * (PresenceChallengeRecord.Size + PresenceProofRecord.Size);
        long serverEgressPerRound = perClient * playerCount;
        Assert.AreEqual(expectedPerClient, perClient);
        Assert.AreEqual(expectedServerEgressPerRound, serverEgressPerRound);
        Assert.AreEqual(expectedServerEgressPerSecond, serverEgressPerRound / 10);
        Assert.AreEqual(expectedChallengeEgressPerSecond, (long)PresenceChallengeRecord.Size * playerCount * playerCount / 10);
        Assert.AreEqual(expectedProofEgressPerSecond, (long)PresenceProofRecord.Size * playerCount * playerCount / 10);

        const int averageTicketBytes = 1024;
        const int refreshIntervalSeconds = 10 * 60;
        long refreshEgressPerSecond = (long)playerCount * playerCount *
            (averageTicketBytes + ProtocolLimits.FrameHeaderSize) / refreshIntervalSeconds;
        Assert.AreEqual(expectedRefreshEgressPerSecond, refreshEgressPerSecond);
        Assert.AreEqual(expectedCombinedEgressPerSecond, serverEgressPerRound / 10 + refreshEgressPerSecond);
        Assert.IsTrue(playerCount < 1_000 || expectedCombinedEgressPerSecond < 20_000_000L);
    }

    private static int DivideRoundUp(int value, int divisor) => (value + divisor - 1) / divisor;
}
