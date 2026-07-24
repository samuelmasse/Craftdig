namespace Craftdig.Identity;

// Development-only issuer key shared between a local dev client (signs) and a local dev server (verifies).
// This is a throwaway key generated on the dev machine; it is never craftdig.io's production signing key,
// and the paths that trust it are gated behind dev-only flags that never enable in a production build.
public sealed class DevIdentityKey
{
    public const string KeyId = "craftdig-dev-key";

    private readonly RSA rsa;

    private DevIdentityKey(RSA rsa) => this.rsa = rsa;

    public static string DefaultPath => Path.Join(Path.GetTempPath(), "craftdig-dev-identity.pkcs8");

    public static DevIdentityKey LoadOrCreate(string path)
    {
        if (!File.Exists(path))
        {
            using var fresh = RSA.Create(2048);
            try
            {
                using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
                stream.Write(fresh.ExportPkcs8PrivateKey());
            }
            catch (IOException)
            {
                // Another dev process created it first; fall through and load the winner.
            }
        }

        var rsa = RSA.Create(2048);
        rsa.ImportPkcs8PrivateKey(File.ReadAllBytes(path), out _);
        return new(rsa);
    }

    public RSA Signer => rsa;

    public SecurityKey PublicKey => new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = KeyId };

    public static Guid PlayerId(string name)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes($"craftdig-dev-identity:{name}"), hash);
        Span<byte> uuid = hash[..16];
        uuid[6] = (byte)((uuid[6] & 0x0F) | 0x40);
        uuid[8] = (byte)((uuid[8] & 0x3F) | 0x80);
        return new Guid(uuid, bigEndian: true);
    }
}
