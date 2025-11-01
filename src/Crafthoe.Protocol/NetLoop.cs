namespace Crafthoe.Protocol;

public class NetLoop
{
    private readonly Dictionary<int, Action<NetSocket, NetMessage>> handlers = [];

    public void Run(NetSocket ns)
    {
        while (true)
        {
            if (!ns.TryGet(out var msg))
                break;

            var handler = handlers[msg.Type] ??
                throw new Exception($"Unrecognized message type: {msg.Type}");

            handler(ns, msg);
        }
    }

    public void Register(int type, Action<NetSocket, NetMessage> handler) =>
        handlers[type] = handler;
}
