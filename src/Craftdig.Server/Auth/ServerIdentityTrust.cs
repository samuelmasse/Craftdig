namespace Craftdig.Server;

[Server]
public class ServerIdentityTrust(
    Log log,
    ServerConfig config,
    ServerPublicContexts publicContexts,
    DevIdentityConfig dev)
    : IdentityTicketValidator(BuildCache(log, config, dev), publicContexts.Allows)
{
    private static IdentityJwksCache BuildCache(Log log, ServerConfig config, DevIdentityConfig dev) =>
        dev.Enabled
            ? IdentityJwksCache.Seeded(log, DevIdentityKey.LoadOrCreate(DevIdentityKey.DefaultPath).PublicKey)
            : new(log, config.IdentityJwksUrl);
}
