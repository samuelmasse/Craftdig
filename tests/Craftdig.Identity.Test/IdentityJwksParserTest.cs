namespace Craftdig;

[TestClass]
public sealed class IdentityJwksParserTest
{
    [TestMethod]
    public void StandardSingleKeySet_ParsesWithoutCustomMetadata()
    {
        string json = $"{{\"keys\":[{CreateKeyJson("permanent-key")}]}}";

        var keys = IdentityJwksCache.ParseKeys(json);

        Assert.AreEqual(1, keys.Count);
        Assert.IsTrue(keys.ContainsKey("permanent-key"));
        Assert.IsFalse(keys.ContainsKey("unknown-key"));
    }

    [TestMethod]
    public void KeySet_RejectsZeroOrMultipleKeysAndPrivateMaterial()
    {
        Assert.ThrowsExactly<InvalidDataException>(() => IdentityJwksCache.ParseKeys("{\"keys\":[]}"));

        string key = CreateKeyJson("first-key");
        string second = CreateKeyJson("second-key");
        Assert.ThrowsExactly<InvalidDataException>(() =>
            IdentityJwksCache.ParseKeys($"{{\"keys\":[{key},{second}]}}"));

        using var rsa = RSA.Create(2048);
        var parameters = rsa.ExportParameters(true);
        string privateKey = CreateKeyJson("private-key", parameters);
        Assert.ThrowsExactly<InvalidDataException>(() =>
            IdentityJwksCache.ParseKeys($"{{\"keys\":[{privateKey}]}}"));
    }

    [TestMethod]
    public void KeySet_RejectsBoundsNoncanonicalEncodingAndInvalidKeyIds()
    {
        byte[] shortModulus = Enumerable.Repeat((byte)0xa5, 255).ToArray();
        Assert.ThrowsExactly<InvalidDataException>(() =>
            IdentityJwksCache.ParseKeys(KeySet("short", Base64UrlEncoder.Encode(shortModulus), StandardExponent())));

        Assert.ThrowsExactly<InvalidDataException>(() =>
            IdentityJwksCache.ParseKeys(KeySet(
                "zero-exponent",
                StandardModulus(),
                Base64UrlEncoder.Encode(new byte[] { 0, 1, 0, 1 }))));

        Assert.ThrowsExactly<InvalidDataException>(() =>
            IdentityJwksCache.ParseKeys(KeySet("padded", StandardModulus() + "=", StandardExponent())));

        Assert.ThrowsExactly<InvalidDataException>(() =>
            IdentityJwksCache.ParseKeys(KeySet("bad kid", StandardModulus(), StandardExponent())));
        Assert.ThrowsExactly<InvalidDataException>(() =>
            IdentityJwksCache.ParseKeys(KeySet("bad/kid", StandardModulus(), StandardExponent())));
    }

    private static string StandardModulus() =>
        Base64UrlEncoder.Encode(Enumerable.Repeat((byte)0xa5, 256).ToArray());

    private static string StandardExponent() => Base64UrlEncoder.Encode(new byte[] { 1, 0, 1 });

    private static string KeySet(string keyId, string modulus, string exponent) =>
        $"{{\"keys\":[{{\"kty\":\"RSA\",\"kid\":\"{keyId}\",\"use\":\"sig\",\"alg\":\"RS256\",\"n\":\"{modulus}\",\"e\":\"{exponent}\"}}]}}";

    private static string CreateKeyJson(string keyId)
    {
        using var rsa = RSA.Create(2048);
        return CreateKeyJson(keyId, rsa.ExportParameters(false));
    }

    private static string CreateKeyJson(string keyId, RSAParameters parameters)
    {
        string modulus = Base64UrlEncoder.Encode(parameters.Modulus!);
        string exponent = Base64UrlEncoder.Encode(parameters.Exponent!);
        string privateExponent = parameters.D == null ? "" : $",\"d\":\"{Base64UrlEncoder.Encode(parameters.D)}\"";
        return $"{{\"kty\":\"RSA\",\"kid\":\"{keyId}\",\"use\":\"sig\",\"alg\":\"RS256\",\"n\":\"{modulus}\",\"e\":\"{exponent}\"{privateExponent}}}";
    }
}
