namespace Craftdig.Client.Security.Test;

[TestClass]
public sealed class PlayerPresenceRoundTest
{
    [TestMethod]
    public void CompleteRound_AcceptsReverseChunkArrivalAndOneExactOwnChallenge()
    {
        var records = ClientTestData.Challenges(3);
        Assert.IsTrue(PresenceRoundDigest.TryCompute(records, out var roundHash));
        var round = new PlayerPresenceRound(Header(roundHash, 0, 2, 3), 1);

        round.AddChunk(Header(roundHash, 1, 2, 3), ClientTestData.Encode(records.AsSpan(2)));
        round.CompleteRound([], 10);
        Assert.IsFalse(round.Complete);

        round.AddChunk(Header(roundHash, 0, 2, 3), ClientTestData.Encode(records.AsSpan(0, 2)));
        round.CompleteRound([(records[1].Sequence, records[1].Nonce, 100L)], 10);

        Assert.IsTrue(round.Complete);
        Assert.IsTrue(round.Usable);
        Assert.IsFalse(round.Invalid);
        Assert.AreEqual(100L, round.LocalDeadline);
        Assert.AreEqual(records[1].SessionId, round.OwnRoundSessionId);
        CollectionAssert.AreEqual(records, round.Records);
    }

    [TestMethod]
    public void CompleteRound_RejectsDuplicateChunkAndCrossChunkRecordReordering()
    {
        var records = ClientTestData.Challenges(2);
        Assert.IsTrue(PresenceRoundDigest.TryCompute(records, out var roundHash));

        var duplicate = new PlayerPresenceRound(Header(roundHash, 0, 2, 2), 1);
        byte[] first = ClientTestData.Encode(records.AsSpan(0, 1));
        duplicate.AddChunk(Header(roundHash, 0, 2, 2), first);
        duplicate.AddChunk(Header(roundHash, 0, 2, 2), first);
        Assert.IsTrue(duplicate.Invalid);
        Assert.IsFalse(duplicate.Usable);
        Assert.AreEqual(PlayerPresenceRoundFault.ChunkMismatch, duplicate.Fault);

        var reordered = new PlayerPresenceRound(Header(roundHash, 0, 2, 2), 2);
        reordered.AddChunk(Header(roundHash, 0, 2, 2), ClientTestData.Encode(records.AsSpan(1, 1)));
        reordered.AddChunk(Header(roundHash, 1, 2, 2), ClientTestData.Encode(records.AsSpan(0, 1)));
        reordered.CompleteRound([(records[0].Sequence, records[0].Nonce, 100L)], 10);
        Assert.IsTrue(reordered.Complete);
        Assert.IsTrue(reordered.Invalid);
        Assert.IsFalse(reordered.Usable);
        Assert.AreEqual(PlayerPresenceRoundFault.DigestMismatch, reordered.Fault);
    }

    [TestMethod]
    public void CompleteRound_StaysIncompleteUntilAllChunksAndRejectsWrongHash()
    {
        var records = ClientTestData.Challenges(2);
        Assert.IsTrue(PresenceRoundDigest.TryCompute(records, out var roundHash));
        var incomplete = new PlayerPresenceRound(Header(roundHash, 0, 2, 2), 1);
        incomplete.AddChunk(Header(roundHash, 0, 2, 2), ClientTestData.Encode(records.AsSpan(0, 1)));
        incomplete.CompleteRound([(records[0].Sequence, records[0].Nonce, 100L)], 10);
        Assert.IsFalse(incomplete.Complete);
        Assert.IsFalse(incomplete.Invalid);
        Assert.IsFalse(incomplete.Usable);

        Hash256 wrongHash = ClientTestData.Hash(99);
        var wrong = new PlayerPresenceRound(Header(wrongHash, 0, 1, 2), 2);
        wrong.AddChunk(Header(wrongHash, 0, 1, 2), ClientTestData.Encode(records));
        wrong.CompleteRound([(records[0].Sequence, records[0].Nonce, 100L)], 10);
        Assert.IsTrue(wrong.Complete);
        Assert.IsTrue(wrong.Invalid);
        Assert.IsFalse(wrong.Usable);
        Assert.AreEqual(PlayerPresenceRoundFault.DigestMismatch, wrong.Fault);
    }

    [TestMethod]
    public void CompleteRound_RequiresOwnChallengeExactlyOnceAndBeforeOriginalDeadline()
    {
        var records = ClientTestData.Challenges(2);
        Assert.IsTrue(PresenceRoundDigest.TryCompute(records, out var roundHash));

        var omitted = Complete(records, roundHash, [(99UL, records[0].Nonce, 100L)], 10);
        Assert.IsTrue(omitted.Complete);
        Assert.IsFalse(omitted.Invalid);
        Assert.IsFalse(omitted.Usable);
        Assert.AreEqual(PlayerPresenceRoundFault.OwnChallengeUnusable, omitted.Fault);

        var repeated = new[]
        {
            records[0],
            records[1] with { Sequence = records[0].Sequence, Nonce = records[0].Nonce },
        };
        Assert.IsTrue(PresenceRoundDigest.TryCompute(repeated, out var repeatedHash));
        var duplicated = Complete(
            repeated,
            repeatedHash,
            [(records[0].Sequence, records[0].Nonce, 100L)],
            10);
        Assert.IsTrue(duplicated.Complete);
        Assert.IsFalse(duplicated.Invalid);
        Assert.IsFalse(duplicated.Usable);
        Assert.AreEqual(PlayerPresenceRoundFault.OwnChallengeUnusable, duplicated.Fault);

        var expired = Complete(
            records,
            roundHash,
            [(records[0].Sequence, records[0].Nonce, 9L)],
            10);
        Assert.IsFalse(expired.Usable);
        Assert.AreEqual(PlayerPresenceRoundFault.OwnChallengeUnusable, expired.Fault);
    }

    private static PlayerPresenceRound Complete(
        PresenceChallengeRecord[] records,
        Hash256 roundHash,
        (ulong Sequence, Nonce256 Nonce, long Deadline)[] pending,
        long now)
    {
        var round = new PlayerPresenceRound(Header(roundHash, 0, 1, (uint)records.Length), 1);
        round.AddChunk(Header(roundHash, 0, 1, (uint)records.Length), ClientTestData.Encode(records));
        round.CompleteRound(pending, now);
        return round;
    }

    private static PresenceRoundChunkHeader Header(
        Hash256 roundHash,
        ushort index,
        ushort count,
        uint total) => new(roundHash, index, count, total);
}
