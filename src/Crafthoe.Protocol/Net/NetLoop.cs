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

    public void Register(int type, Action<NetSocket> handler) =>
        Register(type, (ns, msg) => handler(ns));

    public void Register<C, D>(int type, Action<NetSocket, C, ReadOnlySpan<D>> handler) where C : unmanaged where D : unmanaged
    {
        int header = Marshal.SizeOf<C>();

        Register(type, (ns, msg) =>
        {
            var cmd = MemoryMarshal.AsRef<C>(msg.Data[..header]);
            var data = msg.Data[header..];
            var items = MemoryMarshal.Cast<byte, D>(data);
            handler(ns, cmd, items);
        });
    }

    public void Register<C>(int type, Action<NetSocket, C> handler) where C : unmanaged =>
        Register<C, byte>(type, (ns, cmd, data) => handler(ns, cmd));

    public void Register<C, D>(int type, Action<C, ReadOnlySpan<D>> handler) where C : unmanaged where D : unmanaged =>
        Register<C, D>(type, (ns, cmd, data) => handler(cmd, data));

    public void Register<C>(int type, Action<C> handler) where C : unmanaged =>
        Register<C, byte>(type, (ns, cmd, data) => handler(cmd));

    public void Register<D>(int type, Action<NetSocket, ReadOnlySpan<D>> handler) where D : unmanaged
    {
        Register(type, (ns, msg) =>
        {
            var items = MemoryMarshal.Cast<byte, D>(msg.Data);
            handler(ns, items);
        });
    }

    public void Register<D>(int type, Action<ReadOnlySpan<D>> handler) where D : unmanaged =>
        Register<D>(type, (ns, data) => handler(data));
}
