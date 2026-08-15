namespace Craftdig;

using System.Text.Json;

// Development-only local issuer: signs a real v1 multiplayer ticket with the dev key so the normal
// client and server verification paths run unchanged. Never registered in a production build.
public sealed class DevIdentityTicketIssuer(DevIdentityKey key)
{
    public byte[] Issue(
        Guid playerId,
        string username,
        SessionId sessionId,
        ServerContext context,
        P256PublicKey publicKey,
        DateTimeOffset issuedAt,
        TimeSpan lifetime)
    {
        byte[] xBytes = new byte[Hash256.Size];
        byte[] yBytes = new byte[Hash256.Size];
        publicKey.X.TryWrite(xBytes);
        publicKey.Y.TryWrite(yBytes);

        string header = JsonSerializer.Serialize(new
        {
            alg = "RS256",
            kid = DevIdentityKey.KeyId,
            typ = IdentityTicketFormat.TicketType,
        });
        string claims = JsonSerializer.Serialize(new
        {
            iss = IdentityTicketFormat.Issuer,
            aud = IdentityTicketFormat.Audience,
            sub = playerId.ToString("D"),
            username,
            ver = IdentityTicketFormat.Version,
            sid = sessionId.Value.ToString("D"),
            jti = Guid.NewGuid().ToString("D"),
            iat = issuedAt.ToUnixTimeSeconds(),
            nbf = issuedAt.ToUnixTimeSeconds(),
            exp = issuedAt.Add(lifetime).ToUnixTimeSeconds(),
            server = new { host = context.Host, port = (int)context.Port },
            cnf = new
            {
                jwk = new
                {
                    kty = "EC",
                    crv = "P-256",
                    x = Base64UrlEncoder.Encode(xBytes),
                    y = Base64UrlEncoder.Encode(yBytes),
                },
            },
        });

        string signingInput =
            $"{Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(header))}.{Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(claims))}";
        byte[] signature = key.Signer.SignData(
            Encoding.ASCII.GetBytes(signingInput),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        return Encoding.ASCII.GetBytes($"{signingInput}.{Base64UrlEncoder.Encode(signature)}");
    }
}
