namespace Craftdig;

[Server]
public class ServerIdentityTrust(
    Log log,
    ServerConfig config,
    ServerPublicContexts publicContexts,
    AppDevIdentityConfig dev)
    : IdentityTicketValidator(BuildCache(log, config, dev), publicContexts.Allows)
{
    private static IdentityJwksCache BuildCache(Log log, ServerConfig config, AppDevIdentityConfig dev) =>
        dev.Enabled
            ? IdentityJwksCache.Seeded(log, DevIdentityKey.LoadOrCreate(DevIdentityKey.DefaultPath).PublicKey)
            : new(log, config.IdentityJwksUrl);
}
