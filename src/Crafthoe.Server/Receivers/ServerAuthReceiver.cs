namespace Crafthoe.Server;

using Google.Apis.Auth;

[Server]
public class ServerAuthReceiver(
    ServerIdentities identities,
    SeverAllowlist allowlist,
    ServerSockets sockets)
{
    private readonly GoogleJsonWebSignature.ValidationSettings settings = new()
    {
        Audience = ["253499777147-87jfh5dlbotv4nenq0bd4vtlrd20jo0j.apps.googleusercontent.com"]
    };

    public void Receive(NetSocket ns, ReadOnlySpan<byte> data)
    {
        if (ns.Ent.IsAuthenticated())
        {
            ns.Disconnect();
            return;
        }

        lock (this)
        {
            AuthJwt(ns, Encoding.UTF8.GetString(data));
        }
    }

    private void AuthJwt(NetSocket ns, string token)
    {
        var task = GoogleJsonWebSignature.ValidateAsync(token, settings);
        task.Wait();
        var payload = task.Result;

        if (!payload.EmailVerified)
            throw new Exception();

        var email = payload.Email;
        var iss = payload.Issuer;
        var sub = payload.Subject;
        var uid = $"{iss}|{sub}";

        identities.Verify(email, uid);
        allowlist.Allow(email);

        sockets.ForEach(ns =>
        {
            if (ns.Ent.AuthenticatedUid() == uid)
                ns.Disconnect();
        });

        ns.Ent.AuthenticatedUid() = uid;
        ns.Ent.IsAuthenticated() = true;
    }
}
