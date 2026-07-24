namespace Craftdig.Protocol.Security.Test;

[TestClass]
public sealed class CanonicalEncodingTest
{
    [TestMethod]
    public void UuidCodec_Rfc9562_UsesNetworkByteOrder()
    {
        var value = Guid.Parse("6e355d14-5d89-47dd-9b12-962e6679e750");
        byte[] expected = ProtocolTestData.Bytes("6e355d145d8947dd9b12962e6679e750");
        var actual = new byte[UuidCodec.Size];

        Assert.IsTrue(UuidCodec.TryWrite(value, actual));
        CollectionAssert.AreEqual(expected, actual);
        Assert.IsTrue(UuidCodec.TryRead(actual, out var decoded));
        Assert.AreEqual(value, decoded);
        Assert.IsFalse(UuidCodec.TryRead(actual.AsSpan(1), out _));
        Assert.IsFalse(UuidCodec.TryRead(new byte[UuidCodec.Size + 1], out _));
        Assert.IsFalse(UuidCodec.TryWrite(value, new byte[UuidCodec.Size - 1]));
    }

    [TestMethod]
    public void SessionId_RejectsWrongVersionVariantAndLength()
    {
        byte[] bytes = ProtocolTestData.Bytes("6e355d145d8947dd9b12962e6679e750");
        Assert.IsTrue(SessionId.TryRead(bytes, out var sessionId));
        Assert.AreEqual("6e355d14-5d89-47dd-9b12-962e6679e750", sessionId.ToString());

        byte[] wrongVersion = [.. bytes];
        wrongVersion[6] = (byte)((wrongVersion[6] & 0x0f) | 0x50);
        Assert.IsFalse(SessionId.TryRead(wrongVersion, out _));

        byte[] wrongVariant = [.. bytes];
        wrongVariant[8] &= 0x3f;
        Assert.IsFalse(SessionId.TryRead(wrongVariant, out _));
        Assert.IsFalse(SessionId.TryRead(bytes.AsSpan(1), out _));
        Assert.IsFalse(SessionId.TryRead(new byte[SessionId.Size + 1], out _));
    }

    [TestMethod]
    [DataRow(
        "play.example.com",
        1,
        "43726166746469672053657276657220436f6e7465787420763100010010706c61792e6578616d706c652e636f6d8f44",
        "b1b04344442616b244440624a87823cac6d7078b3539c682027aebdff56a9770")]
    [DataRow(
        "203.0.113.7",
        2,
        "43726166746469672053657276657220436f6e7465787420763100020004cb0071078f44",
        "e37171c99af6e360dde34f8fb39b49d94678660e33d76bc882185b3ae766892b")]
    [DataRow(
        "2001:db8::1",
        3,
        "43726166746469672053657276657220436f6e746578742076310003001020010db80000000000000000000000018f44",
        "d6a5a0143236f138b35335989143aec4f579d5460c951685b28dcd16927f0fb1")]
    public void ServerContext_CanonicalVectors_Match(string host, int expectedKind, string encodedHex, string hashHex)
    {
        Assert.IsTrue(ServerContext.TryParseCanonical(host, 36676, out var context));
        Assert.AreEqual((ServerHostKind)expectedKind, context.HostKind);
        byte[] expectedEncoding = ProtocolTestData.Bytes(encodedHex);
        var actualEncoding = new byte[context.EncodedSize];

        Assert.IsTrue(context.TryWriteCanonical(actualEncoding, out int written));
        Assert.AreEqual(expectedEncoding.Length, written);
        CollectionAssert.AreEqual(expectedEncoding, actualEncoding);
        Assert.AreEqual(hashHex, context.ComputeHash().ToString());
        Assert.IsFalse(context.TryWriteCanonical(new byte[context.EncodedSize - 1], out int shortWritten));
        Assert.AreEqual(0, shortWritten);
    }

    [TestMethod]
    public void ServerContext_Canonicalization_IsStrict()
    {
        Assert.IsTrue(ServerContext.TryCreate("BÜCHER.Example", 36676, out var idn));
        Assert.AreEqual(ServerHostKind.Dns, idn.HostKind);
        Assert.AreEqual("xn--bcher-kva.example", idn.Host);
        Assert.IsTrue(ServerContext.TryParseCanonical("xn--bcher-kva.example", 36676, out _));
        Assert.IsFalse(ServerContext.TryParseCanonical("BÜCHER.Example", 36676, out _));
        Assert.IsFalse(ServerContext.TryParseCanonical("PLAY.EXAMPLE.COM", 36676, out _));

        Assert.IsTrue(ServerContext.TryCreate("2001:0DB8:0:0:0:0:0:1", 36676, out var ipv6));
        Assert.AreEqual("2001:db8::1", ipv6.Host);
        Assert.IsFalse(ServerContext.TryParseCanonical("2001:0DB8:0:0:0:0:0:1", 36676, out _));

        Assert.IsFalse(ServerContext.TryCreate("https://play.example.com", 36676, out _));
        Assert.IsFalse(ServerContext.TryCreate("play.example.com.", 36676, out _));
        Assert.IsFalse(ServerContext.TryCreate(" play.example.com", 36676, out _));
        Assert.IsFalse(ServerContext.TryCreate("[2001:db8::1]", 36676, out _));
        Assert.IsFalse(ServerContext.TryCreate("::ffff:192.0.2.128", 36676, out _));
        Assert.IsFalse(ServerContext.TryParseCanonical("::ffff:c000:280", 36676, out _));
        Assert.IsFalse(ServerContext.TryCreate("play.example.com", 0, out _));
        Assert.IsFalse(ServerContext.TryCreate("play.example.com", 65536, out _));
    }

    [TestMethod]
    public void IdentityPresenceDigests_GoldenVectors_Match()
    {
        Assert.IsTrue(ServerContext.TryParseCanonical("play.example.com", 36676, out var context));
        Hash256 contextHash = context.ComputeHash();
        Hash256 ticketHash = Hash256.Compute(Encoding.ASCII.GetBytes(ProtocolTestData.CompactTicket));
        var serverNonce = ProtocolTestData.Nonce("000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f");

        Assert.AreEqual("a903196b95559d4b25a2f01a9bc40f0ebdfb000b45e41956b05c99be71c532a9", ticketHash.ToString());
        Assert.AreEqual(
            "8098dcbd50219fe1e1bfa07dd3927fc6c01c5f8eebe64b7609a22def816bffbb",
            AuthenticationDigest.Compute(contextHash, serverNonce, ticketHash).ToString());

        PresenceChallengeRecord[] records =
        [
            new(
                ProtocolTestData.Session("00112233-4455-4677-8899-aabbccddeeff"),
                0x0102030405060708,
                ProtocolTestData.Nonce("202122232425262728292a2b2c2d2e2f303132333435363738393a3b3c3d3e3f")),
            new(
                ProtocolTestData.Session("6e355d14-5d89-47dd-9b12-962e6679e750"),
                ulong.MaxValue,
                ProtocolTestData.Nonce("404142434445464748494a4b4c4d4e4f505152535455565758595a5b5c5d5e5f")),
            new(
                ProtocolTestData.Session("ffeeddcc-bbaa-4988-8776-554433221100"),
                42,
                ProtocolTestData.Nonce("a0a1a2a3a4a5a6a7a8a9aaabacadaeafb0b1b2b3b4b5b6b7b8b9babbbcbdbebf")),
        ];

        string[] recordHex =
        [
            "00112233445546778899aabbccddeeff0102030405060708202122232425262728292a2b2c2d2e2f303132333435363738393a3b3c3d3e3f",
            "6e355d145d8947dd9b12962e6679e750ffffffffffffffff404142434445464748494a4b4c4d4e4f505152535455565758595a5b5c5d5e5f",
            "ffeeddccbbaa49888776554433221100000000000000002aa0a1a2a3a4a5a6a7a8a9aaabacadaeafb0b1b2b3b4b5b6b7b8b9babbbcbdbebf",
        ];
        for (int i = 0; i < records.Length; i++)
        {
            var encoded = new byte[PresenceChallengeRecord.Size];
            Assert.IsTrue(records[i].TryWrite(encoded));
            Assert.AreEqual(recordHex[i], Convert.ToHexStringLower(encoded));
        }

        Assert.IsTrue(PresenceRoundDigest.TryCompute(records, out var roundHash));
        Assert.AreEqual("5cdad7d3a18bc03b25571781361d5f2c3df7177898abc484eec05d8dc364b6d6", roundHash.ToString());
        Assert.AreEqual(
            "394475339b47614d424cc2e8b4265fdbdff5eb58383e77537344a2f3c29cb947",
            PresenceProofDigest.Compute(contextHash, roundHash, ticketHash).ToString());
    }

    [TestMethod]
    public void FixedSecurityValues_ReadExactLengthsOnly()
    {
        Assert.IsFalse(Hash256.TryRead(new byte[Hash256.Size - 1], out _));
        Assert.IsFalse(Hash256.TryRead(new byte[Hash256.Size + 1], out _));
        Assert.IsFalse(Nonce256.TryRead(new byte[Nonce256.Size - 1], out _));
        Assert.IsFalse(Nonce256.TryRead(new byte[Nonce256.Size + 1], out _));
        Assert.IsFalse(P256Signature.TryRead(new byte[P256Signature.Size - 1], out _));
        Assert.IsFalse(P256Signature.TryRead(new byte[P256Signature.Size + 1], out _));
        Assert.IsFalse(PresenceChallengeRecord.TryRead(new byte[PresenceChallengeRecord.Size - 1], out _));
        Assert.IsFalse(PresenceChallengeRecord.TryRead(new byte[PresenceChallengeRecord.Size + 1], out _));
    }
}
