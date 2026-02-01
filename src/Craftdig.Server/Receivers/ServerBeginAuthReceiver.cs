namespace Craftdig.Server;

[Server]
public class ServerBeginAuthReceiver(AppLog log)
{
    public void Receive(NetSocket ns)
    {
        lock (this)
        {
            if (ns.Ent.IsAuthenticated())
            {
                log.Warn("Socket {0} tried to re-authenticate", ns.Ent.Tag());
                ns.Disconnect();
                return;
            }

            if (ns.Ent.AuthNonce() != null)
            {
                log.Warn("Socket {0} tried to get a new nonce", ns.Ent.Tag());
                ns.Disconnect();
                return;
            }

            Span<byte> bytes = stackalloc byte[32];
            RandomNumberGenerator.Fill(bytes);
            var nonce = Convert.ToHexStringLower(bytes);

            log.Error("Generated nonce {0}", nonce);

            ns.Send<ReadyAuthCommand, byte>(Encoding.UTF8.GetBytes(nonce));
            ns.Ent.AuthNonce() = nonce;
        }
    }
}
