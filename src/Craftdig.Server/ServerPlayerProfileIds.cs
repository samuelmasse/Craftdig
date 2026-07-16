namespace Craftdig.Server;

[Server]
public class ServerPlayerProfileIds
{
    public Guid Get(string uid)
    {
        if (uid[0] != '#')
            return Guid.Parse(uid);

        var bytes = Encoding.UTF8.GetBytes(uid);
        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(bytes, hash);
        return new(hash[..16]);
    }
}
