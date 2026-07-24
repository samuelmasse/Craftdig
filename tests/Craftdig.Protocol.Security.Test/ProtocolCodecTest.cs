namespace Craftdig.Protocol.Security.Test;

[TestClass]
public sealed class ProtocolCodecTest
{
    [TestMethod]
    public void AuthenticationCodecs_EnforceExactBoundaries()
    {
        var nonce = ProtocolTestData.Nonce("000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f");
        var readyBuffer = new byte[ReadyAuthCommandCodec.Size + 1];
        Assert.IsTrue(ReadyAuthCommandCodec.TryWrite(readyBuffer, nonce, out int readyWritten));
        Assert.AreEqual(ReadyAuthCommandCodec.Size, readyWritten);
        Assert.IsTrue(ReadyAuthCommandCodec.TryRead(readyBuffer.AsSpan(0, readyWritten), out var decodedNonce));
        Assert.AreEqual(nonce, decodedNonce);
        Assert.IsFalse(ReadyAuthCommandCodec.TryRead(readyBuffer.AsSpan(0, readyWritten - 1), out _));
        Assert.IsFalse(ReadyAuthCommandCodec.TryRead(readyBuffer, out _));

        byte[] ticket = Encoding.ASCII.GetBytes(ProtocolTestData.CompactTicket);
        byte[] signatureBytes = Enumerable.Repeat((byte)0xa5, P256Signature.Size).ToArray();
        Assert.IsTrue(P256Signature.TryRead(signatureBytes, out var signature));
        var completeBuffer = new byte[ticket.Length + P256Signature.Size + 1];
        Assert.IsTrue(CompleteAuthCommandCodec.TryWrite(completeBuffer, ticket, signature, out int completeWritten));
        Assert.AreEqual(ticket.Length + P256Signature.Size, completeWritten);
        Assert.IsTrue(CompleteAuthCommandCodec.TryRead(
            completeBuffer.AsSpan(0, completeWritten),
            out var decodedTicket,
            out var decodedSignature));
        Assert.IsTrue(ticket.AsSpan().SequenceEqual(decodedTicket));
        Assert.AreEqual(signature, decodedSignature);
        Assert.IsFalse(CompleteAuthCommandCodec.TryRead(new byte[P256Signature.Size], out _, out _));
        Assert.IsFalse(CompleteAuthCommandCodec.TryRead(completeBuffer, out _, out _));

        string maximumTicketText = $"a.{new string('b', ProtocolLimits.MaxIdentityTicketSize - 4)}.c";
        byte[] maximumTicket = Encoding.ASCII.GetBytes(maximumTicketText);
        Assert.AreEqual(ProtocolLimits.MaxIdentityTicketSize, maximumTicket.Length);
        var maximumComplete = new byte[CompleteAuthCommandCodec.MaxSize];
        Assert.IsTrue(CompleteAuthCommandCodec.TryWrite(maximumComplete, maximumTicket, signature, out int maximumWritten));
        Assert.AreEqual(CompleteAuthCommandCodec.MaxSize, maximumWritten);

        byte[] oversizedTicket = [.. maximumTicket, (byte)'d'];
        Assert.IsFalse(CompleteAuthCommandCodec.TryWrite(
            new byte[oversizedTicket.Length + P256Signature.Size],
            oversizedTicket,
            signature,
            out _));
    }

    [TestMethod]
    public void PlayerIdentityCodec_RejectsMalformedAndOversizedTickets()
    {
        byte[] ticket = Encoding.ASCII.GetBytes(ProtocolTestData.CompactTicket);
        var destination = new byte[ticket.Length + 1];
        Assert.IsTrue(PlayerIdentityCommandCodec.TryWrite(destination, ticket, out int written));
        Assert.AreEqual(ticket.Length, written);
        Assert.IsTrue(PlayerIdentityCommandCodec.TryRead(destination.AsSpan(0, written), out var decoded));
        Assert.IsTrue(ticket.AsSpan().SequenceEqual(decoded));

        Assert.IsFalse(PlayerIdentityCommandCodec.TryRead("a..c"u8, out _));
        Assert.IsFalse(PlayerIdentityCommandCodec.TryRead("a.b.c.d"u8, out _));
        Assert.IsFalse(PlayerIdentityCommandCodec.TryRead("a.b.c="u8, out _));
        Assert.IsFalse(PlayerIdentityCommandCodec.TryRead("a.b.c!"u8, out _));
        Assert.IsFalse(PlayerIdentityCommandCodec.TryRead(new byte[ProtocolLimits.MaxIdentityTicketSize + 1], out _));
        Assert.IsFalse(PlayerIdentityCommandCodec.TryWrite(new byte[ticket.Length - 1], ticket, out _));
    }

    [TestMethod]
    public void PresenceClientCodecs_EnforceExactLengths()
    {
        var nonce = ProtocolTestData.Nonce("202122232425262728292a2b2c2d2e2f303132333435363738393a3b3c3d3e3f");
        var challenge = new PresenceChallenge(0x0102030405060708, nonce);
        var challengeBuffer = new byte[PresenceChallengeCommandCodec.Size + 1];
        Assert.IsTrue(PresenceChallengeCommandCodec.TryWrite(challengeBuffer, challenge, out int challengeWritten));
        Assert.AreEqual(PresenceChallengeCommandCodec.Size, challengeWritten);
        Assert.AreEqual("0102030405060708", Convert.ToHexStringLower(challengeBuffer.AsSpan(0, sizeof(ulong))));
        Assert.IsTrue(PresenceChallengeCommandCodec.TryRead(
            challengeBuffer.AsSpan(0, challengeWritten),
            out var decodedChallenge));
        Assert.AreEqual(challenge, decodedChallenge);
        Assert.IsFalse(PresenceChallengeCommandCodec.TryRead(challengeBuffer.AsSpan(0, challengeWritten - 1), out _));
        Assert.IsFalse(PresenceChallengeCommandCodec.TryRead(challengeBuffer, out _));

        var roundHash = ProtocolTestData.Hash("5cdad7d3a18bc03b25571781361d5f2c3df7177898abc484eec05d8dc364b6d6");
        var ticketHash = ProtocolTestData.Hash("a903196b95559d4b25a2f01a9bc40f0ebdfb000b45e41956b05c99be71c532a9");
        byte[] signatureBytes = Enumerable.Range(0, P256Signature.Size).Select(static i => (byte)(i + 1)).ToArray();
        Assert.IsTrue(P256Signature.TryRead(signatureBytes, out var signature));
        var proof = new PresenceProof(roundHash, ticketHash, signature);
        var proofBuffer = new byte[PresenceProofCommandCodec.Size + 1];
        Assert.IsTrue(PresenceProofCommandCodec.TryWrite(proofBuffer, proof, out int proofWritten));
        Assert.AreEqual(PresenceProofCommandCodec.Size, proofWritten);
        Assert.IsTrue(PresenceProofCommandCodec.TryRead(proofBuffer.AsSpan(0, proofWritten), out var decodedProof));
        Assert.AreEqual(proof, decodedProof);
        Assert.IsFalse(PresenceProofCommandCodec.TryRead(proofBuffer.AsSpan(0, proofWritten - 1), out _));
        Assert.IsFalse(PresenceProofCommandCodec.TryRead(proofBuffer, out _));
    }

    [TestMethod]
    public void PresenceRoundChunkCodec_ValidatesOrderAndHeader()
    {
        PresenceChallengeRecord[] records = ProtocolTestData.Challenges(3);
        Assert.IsTrue(PresenceRoundDigest.TryCompute(records, out var roundHash));
        var header = new PresenceRoundChunkHeader(roundHash, 0, 1, (uint)records.Length);
        var buffer = new byte[PresenceRoundChunkCommandCodec.MaxSize];

        Assert.IsTrue(PresenceRoundChunkCommandCodec.TryWrite(buffer, header, records, out int written));
        Assert.AreEqual(PresenceRoundChunkCommandCodec.HeaderSize + records.Length * PresenceChallengeRecord.Size, written);
        Assert.IsTrue(PresenceRoundChunkCommandCodec.TryRead(
            buffer.AsSpan(0, written),
            out var decodedHeader,
            out var recordBytes));
        Assert.AreEqual(header, decodedHeader);
        Assert.AreEqual(records.Length * PresenceChallengeRecord.Size, recordBytes.Length);
        for (int i = 0; i < records.Length; i++)
        {
            Assert.IsTrue(WireRecords.TryReadAt(recordBytes, i, out PresenceChallengeRecord record));
            Assert.AreEqual(records[i], record);
        }

        Assert.IsFalse(WireRecords.TryReadAt(recordBytes, -1, out PresenceChallengeRecord _));
        Assert.IsFalse(WireRecords.TryReadAt(recordBytes, records.Length, out PresenceChallengeRecord _));
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryRead(buffer.AsSpan(0, written - 1), out _, out _));
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryRead(buffer.AsSpan(0, written + 1), out _, out _));
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryRead(new byte[PresenceRoundChunkCommandCodec.HeaderSize], out _, out _));

        PresenceChallengeRecord[] reversed = [.. records.Reverse()];
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryWrite(buffer, header, reversed, out _));
        PresenceChallengeRecord[] duplicateSession = [records[0], records[0] with { Sequence = 1 }];
        var duplicateHeader = header with { TotalChallengeCount = 2 };
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryWrite(buffer, duplicateHeader, duplicateSession, out _));
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryWrite(buffer, header with { ChunkCount = 0 }, records, out _));
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryWrite(buffer, header with { ChunkIndex = 1 }, records, out _));
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryWrite(buffer, header with { TotalChallengeCount = 0 }, records, out _));
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryWrite(
            buffer,
            header with { TotalChallengeCount = ProtocolLimits.MaxPresencePlayers + 1 },
            records,
            out _));
    }

    [TestMethod]
    public void PresenceBatchCodecs_EnforceRecordCaps()
    {
        Assert.IsTrue(PresenceRoundChunkCommandCodec.MaxSize <= ProtocolLimits.MaxMessageSize);
        Assert.IsTrue(PresenceProofBatchCommandCodec.MaxSize <= ProtocolLimits.MaxMessageSize);

        PresenceChallengeRecord[] maximumChallenges =
            ProtocolTestData.Challenges(PresenceRoundChunkCommandCodec.MaxRecordCount);
        Assert.IsTrue(PresenceRoundDigest.TryCompute(maximumChallenges, out var roundHash));
        var challengeHeader = new PresenceRoundChunkHeader(roundHash, 0, 1, (uint)maximumChallenges.Length);
        var challengeBuffer = new byte[PresenceRoundChunkCommandCodec.MaxSize];
        Assert.IsTrue(PresenceRoundChunkCommandCodec.TryWrite(
            challengeBuffer,
            challengeHeader,
            maximumChallenges,
            out int challengeWritten));
        Assert.AreEqual(PresenceRoundChunkCommandCodec.MaxSize, challengeWritten);
        Assert.IsTrue(PresenceRoundChunkCommandCodec.TryRead(challengeBuffer, out _, out _));

        PresenceChallengeRecord[] tooManyChallenges =
            ProtocolTestData.Challenges(PresenceRoundChunkCommandCodec.MaxRecordCount + 1);
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryWrite(
            new byte[PresenceRoundChunkCommandCodec.HeaderSize + tooManyChallenges.Length * PresenceChallengeRecord.Size],
            challengeHeader with { TotalChallengeCount = (uint)tooManyChallenges.Length },
            tooManyChallenges,
            out _));
        var oversizedChallengePayload = new byte[
            PresenceRoundChunkCommandCodec.HeaderSize + tooManyChallenges.Length * PresenceChallengeRecord.Size];
        roundHash.TryWrite(oversizedChallengePayload);
        BinaryPrimitives.WriteUInt16BigEndian(oversizedChallengePayload.AsSpan(Hash256.Size), 0);
        BinaryPrimitives.WriteUInt16BigEndian(oversizedChallengePayload.AsSpan(Hash256.Size + sizeof(ushort)), 1);
        BinaryPrimitives.WriteUInt32BigEndian(
            oversizedChallengePayload.AsSpan(Hash256.Size + sizeof(ushort) * 2),
            (uint)tooManyChallenges.Length);
        Assert.IsFalse(PresenceRoundChunkCommandCodec.TryRead(oversizedChallengePayload, out _, out _));

        PresenceProofRecord[] maximumProofs = ProtocolTestData.Proofs(PresenceProofBatchCommandCodec.MaxRecordCount);
        var proofBuffer = new byte[PresenceProofBatchCommandCodec.MaxSize];
        Assert.IsTrue(PresenceProofBatchCommandCodec.TryWrite(proofBuffer, roundHash, maximumProofs, out int proofWritten));
        Assert.AreEqual(PresenceProofBatchCommandCodec.MaxSize, proofWritten);
        Assert.IsTrue(PresenceProofBatchCommandCodec.TryRead(proofBuffer, out _, out _));

        PresenceProofRecord[] tooManyProofs = ProtocolTestData.Proofs(PresenceProofBatchCommandCodec.MaxRecordCount + 1);
        Assert.IsFalse(PresenceProofBatchCommandCodec.TryWrite(
            new byte[PresenceProofBatchCommandCodec.HeaderSize + tooManyProofs.Length * PresenceProofRecord.Size],
            roundHash,
            tooManyProofs,
            out _));
        var oversizedProofPayload = new byte[
            PresenceProofBatchCommandCodec.HeaderSize + tooManyProofs.Length * PresenceProofRecord.Size];
        roundHash.TryWrite(oversizedProofPayload);
        Assert.IsFalse(PresenceProofBatchCommandCodec.TryRead(oversizedProofPayload, out _, out _));
    }

    [TestMethod]
    public void PresenceProofBatchCodec_HandlesCompleteRecordsOnly()
    {
        var roundHash = ProtocolTestData.Hash("5cdad7d3a18bc03b25571781361d5f2c3df7177898abc484eec05d8dc364b6d6");
        PresenceProofRecord[] records = ProtocolTestData.Proofs(3);
        var buffer = new byte[PresenceProofBatchCommandCodec.MaxSize];
        Assert.IsTrue(PresenceProofBatchCommandCodec.TryWrite(buffer, roundHash, records, out int written));
        Assert.IsTrue(PresenceProofBatchCommandCodec.TryRead(
            buffer.AsSpan(0, written),
            out var decodedRoundHash,
            out var recordBytes));
        Assert.AreEqual(roundHash, decodedRoundHash);
        Assert.AreEqual(records.Length * PresenceProofRecord.Size, recordBytes.Length);
        for (int i = 0; i < records.Length; i++)
        {
            Assert.IsTrue(WireRecords.TryReadAt(recordBytes, i, out PresenceProofRecord record));
            Assert.AreEqual(records[i], record);
        }

        Assert.IsFalse(WireRecords.TryReadAt(recordBytes, -1, out PresenceProofRecord _));
        Assert.IsFalse(WireRecords.TryReadAt(recordBytes, records.Length, out PresenceProofRecord _));
        Assert.IsFalse(PresenceProofBatchCommandCodec.TryRead(buffer.AsSpan(0, written - 1), out _, out _));
        Assert.IsFalse(PresenceProofBatchCommandCodec.TryRead(buffer.AsSpan(0, written + 1), out _, out _));

        var emptyBuffer = new byte[PresenceProofBatchCommandCodec.HeaderSize];
        Assert.IsTrue(PresenceProofBatchCommandCodec.TryWrite(emptyBuffer, roundHash, [], out int emptyWritten));
        Assert.AreEqual(PresenceProofBatchCommandCodec.HeaderSize, emptyWritten);
        Assert.IsTrue(PresenceProofBatchCommandCodec.TryRead(emptyBuffer, out _, out var emptyRecords));
        Assert.AreEqual(0, emptyRecords.Length);
    }
}
