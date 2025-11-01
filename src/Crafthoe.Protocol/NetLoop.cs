namespace Crafthoe.World;

public class NetLoop
{
    private Action<NetSocket, NetMessage>?[] handlers = [];

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

    public void Register(int type, Action<NetSocket, NetMessage> handler)
    {
        if (handlers.Length <= type)
            Array.Resize(ref handlers, (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)type + 1));

        handlers[type] = handler;
    }
}
