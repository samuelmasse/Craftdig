namespace Craftdig.Identity.Test;

[TestClass]
public sealed class StrictJsonTest
{
    [TestMethod]
    [DataRow("1", true, 1L)]
    [DataRow("2147483647", true, 2_147_483_647L)]
    [DataRow("9223372036854775807", true, long.MaxValue)]
    [DataRow("0", false, 0L)]
    [DataRow("-1", false, 0L)]
    [DataRow("1.0", false, 0L)]
    [DataRow("1e0", false, 0L)]
    [DataRow("9223372036854775808", false, 0L)]
    public void CanonicalInt64_RejectsSignsFractionsExponentsZeroAndOverflow(
        string literal,
        bool expected,
        long expectedValue)
    {
        using var document = JsonDocument.Parse($"{{\"value\":{literal}}}");
        bool result = StrictJson.TryGetCanonicalInt64(document.RootElement, "value", out long value);

        Assert.AreEqual(expected, result);
        if (expected)
            Assert.AreEqual(expectedValue, value);
    }

    [TestMethod]
    public void UniqueProperties_RejectDuplicatesAndMissingNames()
    {
        using var document = JsonDocument.Parse("{\"a\":1,\"b\":2,\"a\":3}");
        var root = document.RootElement;

        Assert.IsFalse(StrictJson.TryGetUnique(root, "a", out _));
        Assert.IsTrue(StrictJson.TryGetUnique(root, "b", out var unique));
        Assert.AreEqual(2, unique.GetInt32());
        Assert.IsFalse(StrictJson.TryGetUnique(root, "c", out _));
        Assert.IsFalse(StrictJson.HasPropertyCount(root, 2));
        Assert.IsTrue(StrictJson.HasPropertyCount(root, 3));
    }

    [TestMethod]
    public void CanonicalUuidAndBase64Url_RejectNoncanonicalForms()
    {
        Assert.IsTrue(StrictJson.IsCanonicalUuid("0d1f2a3b-4c5d-4e6f-8a9b-0c1d2e3f4a5b", out _));
        Assert.IsFalse(StrictJson.IsCanonicalUuid("0D1F2A3B-4C5D-4E6F-8A9B-0C1D2E3F4A5B", out _));
        Assert.IsFalse(StrictJson.IsCanonicalUuid("00000000-0000-0000-0000-000000000000", out _));
        Assert.IsFalse(StrictJson.IsCanonicalUuid("{0d1f2a3b-4c5d-4e6f-8a9b-0c1d2e3f4a5b}", out _));

        Assert.IsTrue(StrictJson.TryDecodeCanonicalBase64Url("aGVsbG8", out byte[]? decoded));
        Assert.AreEqual("hello", Encoding.ASCII.GetString(decoded));
        Assert.IsFalse(StrictJson.TryDecodeCanonicalBase64Url("aGVsbG8=", out _));
        Assert.IsFalse(StrictJson.TryDecodeCanonicalBase64Url("aGVsbG9", out _));
    }
}
