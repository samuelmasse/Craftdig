namespace Craftdig.Identity;

// Dev-only switch: when Enabled, the client signs tickets with a local dev key and both client and
// server trust that key instead of craftdig.io. Defaults to disabled and is only turned on by dev
// composition scripts, so a production build never trusts the dev key.
[App]
public class DevIdentityConfig
{
    public bool Enabled { get; init; }
    public string? Name { get; init; }
}
