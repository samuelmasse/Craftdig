namespace Craftdig;

[Module]
public class ModuleIdentityTrust(Log log, DevIdentityConfig dev) : IdentityTicketValidator(BuildCache(log, dev))
{
    private const string JwksUrl = "https://craftdig.io/.well-known/jwks.json";

    private static IdentityJwksCache BuildCache(Log log, DevIdentityConfig dev) =>
        dev.Enabled
            ? IdentityJwksCache.Seeded(log, DevIdentityKey.LoadOrCreate(DevIdentityKey.DefaultPath).PublicKey)
            : new(log, JwksUrl);
}
