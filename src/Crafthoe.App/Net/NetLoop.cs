namespace Crafthoe.App;

public class NetLoop
{
    private Action<NetMessage>?[] handlers = [];

    public void Run(NetSocket ns)
    {
        while (true)
        {
            if (!ns.TryGet(out var msg))
                break;

            var handler = handlers[msg.Type];
            if (handler == null)
                break;

            handler(msg);
        }
    }

    public void Register(int type, Action<NetMessage> handler)
    {
        if (handlers.Length <= type)
            Array.Resize(ref handlers, (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)type + 1));

        handlers[type] = handler;
    }
}
