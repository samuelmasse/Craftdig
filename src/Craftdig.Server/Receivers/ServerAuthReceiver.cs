namespace Craftdig.Server;

using Google.Apis.Auth;

[Server]
public class ServerAuthReceiver(
    AppLog log,
    ServerIdentities identities,
    SeverAllowlist allowlist,
    ServerSockets sockets,
    ServerClientLimits clientLimits)
{
    private readonly GoogleJsonWebSignature.ValidationSettings settings = new()
    {
        Audience = ["253499777147-87jfh5dlbotv4nenq0bd4vtlrd20jo0j.apps.googleusercontent.com"]
    };

    public void Receive(NetSocket ns, ReadOnlySpan<byte> data)
    {
        if (ns.Ent.IsAuthenticated())
        {
            log.Warn("Socket {0} tried to re-authenticate", ns.Ent.Tag());
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
        GoogleJsonWebSignature.Payload payload;

        try
        {
            var task = GoogleJsonWebSignature.ValidateAsync(token, settings);
            task.Wait();
            payload = task.Result;
        }
        catch
        {
            log.Warn("Socket {0} tried to authenticate with an invalid JWT", ns.Ent.Tag());
            ns.Disconnect();
            return;
        }

        var email = payload.Email;
        var iss = payload.Issuer;
        var sub = payload.Subject;
        var uid = $"{iss}|{sub}";

        if (!payload.EmailVerified)
        {
            log.Warn("Socket {0} tried to authenticate with an unverified email : {1}",
                ns.Ent.Tag(), email);

            ns.Disconnect();
            return;
        }

        if (!identities.Verify(email, uid))
        {
            log.Warn("Socket {0} tried to authenticate with an inconsitent identity : {1} but {2}",
                ns.Ent.Tag(), email, uid);

            ns.Disconnect();
            return;
        }

        if (!allowlist.Allow(email))
        {
            log.Warn("Socket {0} tried to authenticate with an email that is not allowlisted : {1}",
                ns.Ent.Tag(), email);

            ns.Disconnect();
            return;
        }

        sockets.ForEach(ns =>
        {
            if (ns.Ent.AuthenticatedUid() == uid)
                ns.Disconnect();
        });

        log.Info("Socket {0} authenticated : {1}", ns.Ent.Tag(), email);

        ns.Ent.Tag() = email;
        ns.Ent.AuthenticatedEmail() = email;
        ns.Ent.AuthenticatedUid() = uid;
        ns.Ent.IsAuthenticated() = true;
        clientLimits.Pulse();
    }
}
