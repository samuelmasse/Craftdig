namespace Craftdig.Server;

using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

[Server]
public class ServerAuth(AppLog log, ServerSockets sockets, ServerClientLimits clientLimits, SeverAllowlist allowlist)
{
    private readonly HttpClient http = new();
    private readonly JwtSecurityTokenHandler jwts = new();
    private IReadOnlyList<SecurityKey>? jwksKeys;
    private DateTime jwksKeysExpiresAtUtc;

    public void SendNonce(NetSocket ns)
    {
        // Getting a nonce when already authenticated is not allowed
        if (GuardAlreadyAuthenticated(ns))
            return;

        // Getting a new nonce is not allowed
        if (GuardNonceExists(ns))
            return;

        // Generate a random nonce
        var nonce = GenerateNonce();
        log.Info("Socket {0} generated nonce", ns.Ent.Tag());

        // Remember the nonce for this socket
        ns.Ent.AuthNonce() = nonce;

        // Send the nonce to the client to be included in future JWT
        ns.Send<ReadyAuthCommand, byte>(Encoding.UTF8.GetBytes(nonce));
    }

    public void AuthenticateJwt(NetSocket ns, ReadOnlySpan<byte> data)
    {
        // Authenticating a second time is not allowed
        if (GuardAlreadyAuthenticated(ns))
            return;

        // To authenticate the client must have requested a nonce prior
        if (GuardNonceNotSet(ns))
            return;

        var jwt = Encoding.UTF8.GetString(data);

        // Online validation of the JWT with auth service
        if (GuardInvalidJwt(ns, jwt, out var token))
            return;

        // Check if the nonce associated with this valid JWT is the nonce we sent before
        if (GuardInvalidNonce(ns, token))
            return;

        // At this point the auth worked and we extract the user info
        ExtractUidAndUsername(token, out var uid, out var username);

        // Prevent connection to non allowlisted users
        if (GuardAllowlist(ns, uid, username))
            return;

        // Limit sessions of the same player to one at a time
        // Username is globally unique but can change during a session
        // To limit confusion we kick any player with the same username
        // This can only happen if a user changed their username during
        // a session and there was another user that took their username
        DisconnectSocketsWithSameUidOrUsername(uid, username);

        // Strongly associate this socket as being authenticated with this uid and username
        ns.Ent.AuthenticatedUid() = uid;
        ns.Ent.AuthenticatedUsername() = username;
        // After this is set to true a client is allowed to ask to spawn
        ns.Ent.IsAuthenticated() = true;
        log.Info("Socket {0} authenticated : {1}", ns.Ent.Tag(), uid);
        log.Info("Socket {0} username : {1}", ns.Ent.Tag(), username);

        // Forget nonce
        ns.Ent.AuthNonce() = null;

        // Also set a friendly tag for this socket to the username
        ns.Ent.Tag() = username;

        // Let the client know that the auth was successful
        ns.Send<ResultAuthCommand>();

        // There is one less unauth socket now
        clientLimits.Pulse();
    }

    public void AuthenticateNoAuth(NetSocket ns, ReadOnlySpan<byte> data)
    {
        // Authenticating a second time is not allowed
        if (GuardAlreadyAuthenticated(ns))
            return;

        // For no auth we just trust the client to send a uid that we use as both username and uid
        // We surround the uid with # to dinstinguish it from legitimate players
        // This is meant for testing by server administrators
        // No auth access can be opened on a specific port that only they have access to
        var uid = $"#{Encoding.UTF8.GetString(data)}#";
        DisconnectSocketsWithSameUidOrUsername(uid, uid);

        // Strongly associate this socket as being authenticated with this uid and username
        ns.Ent.AuthenticatedUid() = uid;
        ns.Ent.AuthenticatedUsername() = uid;
        ns.Ent.IsAuthenticated() = true;
        log.Info("Socket {0} no auth authenticated : {1}", ns.Ent.Tag(), uid);

        // Also set a friendly tag for this socket to the username
        ns.Ent.Tag() = uid;

        // Let the client know that the auth was successful
        ns.Send<ResultAuthCommand>();

        // There is one less unauth socket now
        clientLimits.Pulse();
    }

    private string GenerateNonce()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexStringLower(bytes);
    }

    private bool GuardAlreadyAuthenticated(NetSocket ns)
    {
        if (ns.Ent.IsAuthenticated())
        {
            log.Warn("Socket {0} tried to re-authenticate", ns.Ent.Tag());
            ns.Disconnect();
            return true;
        }
        else return false;
    }

    private bool GuardNonceExists(NetSocket ns)
    {
        if (ns.Ent.AuthNonce() != null)
        {
            log.Warn("Socket {0} tried to get a new nonce", ns.Ent.Tag());
            ns.Disconnect();
            return true;
        }
        else return false;
    }

    private bool GuardNonceNotSet(NetSocket ns)
    {
        if (ns.Ent.AuthNonce() == null)
        {
            log.Warn("Socket {0} tried to auth without a nonce", ns.Ent.Tag());
            ns.Disconnect();
            return true;
        }
        else return false;
    }

    private bool GuardInvalidJwt(NetSocket ns, string jwt, [NotNullWhen(false)] out JwtSecurityToken? token)
    {
        int attempt = 0;

        while (attempt < 2)
        {
            try
            {
                attempt++;

                var keys = GetJwksKeys(attempt > 1);

                jwts.ValidateToken(jwt, new()
                {
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = keys,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    RequireExpirationTime = true
                }, out var validated);

                token = (JwtSecurityToken)validated;
                return false;
            }
            catch { }
        }

        log.Warn("Socket {0} tried to authenticate with an invalid JWT", ns.Ent.Tag());
        ns.Disconnect();
        token = null;
        return true;
    }

    private IReadOnlyList<SecurityKey> GetJwksKeys(bool forceRefresh)
    {
        var nowUtc = DateTime.UtcNow;

        if (!forceRefresh && jwksKeys != null && nowUtc < jwksKeysExpiresAtUtc)
            return jwksKeys;

        var jwksJsonTask = http.GetStringAsync("https://craftdig.io/.well-known/jwks.json");
        jwksJsonTask.Wait();

        var jwksJson = jwksJsonTask.Result;
        var jwks = new JsonWebKeySet(jwksJson);
        var keys = jwks.Keys.Cast<SecurityKey>().ToArray();

        jwksKeys = keys;
        jwksKeysExpiresAtUtc = nowUtc.AddHours(1);

        return jwksKeys;
    }

    private bool GuardInvalidNonce(NetSocket ns, JwtSecurityToken token)
    {
        var nonceClaim = token.Claims.FirstOrDefault(x => x.Type == "nonce");

        if (nonceClaim == null || nonceClaim.Value != ns.Ent.AuthNonce())
        {
            log.Warn("Socket {0} tried to authenticate with a wrong nonce", ns.Ent.Tag());
            ns.Disconnect();
            return true;
        }
        else return false;
    }

    private bool GuardAllowlist(NetSocket ns, string uid, string username)
    {
        // The allowlist works for either uid or username
        // A username cannot be a valid uid as the '-' character is not
        // allowed for usernames and is always used for uids which are UUIDs

        if (!allowlist.Allow(uid) && !allowlist.Allow(username))
        {
            log.Warn("Socket {0} tried to join but is not allowisted by either {1} or {2}",
                ns.Ent.Tag(), uid, username);

            ns.Disconnect();
            return true;
        }
        else return false;
    }

    private void ExtractUidAndUsername(JwtSecurityToken token, out string uid, out string username)
    {
        var usernameClaim = token.Claims.First(x => x.Type == "username");

        uid = token.Subject;
        username = usernameClaim.Value;
    }

    private void DisconnectSocketsWithSameUidOrUsername(string uid, string username)
    {
        sockets.ForEach(ns =>
        {
            if (ns.Ent.AuthenticatedUid() == uid || ns.Ent.AuthenticatedUsername() == username)
            {
                ns.Disconnect();
                log.Info("Socket {0} kicked due to conflict", ns.Ent.Tag());
            }
        });
    }

    private record ValidateTokenResponse(bool Valid);
}
