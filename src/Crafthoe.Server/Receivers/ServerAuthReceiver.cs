namespace Crafthoe.Server;

using Google.Apis.Auth;

[Server]
public class ServerAuthReceiver
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
        var payload = GoogleJsonWebSignature.ValidateAsync(token, settings);
        payload.Wait();
        ns.Ent.IsAuthenticated() = true;
    }
}
