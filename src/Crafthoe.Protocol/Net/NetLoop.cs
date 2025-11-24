namespace Crafthoe.Protocol;

public class NetLoop(AppLog log)
{
    private readonly Action<NetSocket, NetMessage>[] handlers = new Action<NetSocket, NetMessage>[0xFFFF];

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

    public void Register(ushort type, Action<NetSocket, NetMessage> handler) =>
        handlers[type] = handler;

    public void Register<C, D>(Action<NetSocket, C, ReadOnlySpan<D>> handler)
        where C : unmanaged, ICommand where D : unmanaged
    {
        int header = Marshal.SizeOf<C>();

        Register(C.CommandId, (ns, msg) =>
        {
            log.Trace("Socket {0} <- {1} ({2}) {3} bytes", ns.Ent.Tag(), typeof(C).Name, C.CommandId, msg.Data.Length);

            var cmd = MemoryMarshal.AsRef<C>(msg.Data[..header]);
            var data = msg.Data[header..];
            var items = MemoryMarshal.Cast<byte, D>(data);
            handler(ns, cmd, items);
        });
    }

    public void Register<C>(Action<NetSocket> handler)
        where C : unmanaged, ICommand =>
        Register<C, byte>((ns, cmd, data) => handler(ns));

    public void Register<C>(Action<NetSocket, C> handler)
        where C : unmanaged, ICommand =>
        Register<C, byte>((ns, cmd, data) => handler(ns, cmd));

    public void Register<C, D>(Action<NetSocket, ReadOnlySpan<D>> handler)
        where C : unmanaged, ICommand where D : unmanaged =>
        Register<C, D>((ns, cmd, items) => handler(ns, items));

    public void Register<C>(Action handler)
        where C : unmanaged, ICommand =>
        Register<C, byte>((ns, cmd, data) => handler());

    public void Register<C>(Action<C> handler)
        where C : unmanaged, ICommand =>
        Register<C, byte>((ns, cmd, data) => handler(cmd));

    public void Register<C, D>(Action<C, ReadOnlySpan<D>> handler)
        where C : unmanaged, ICommand where D : unmanaged =>
        Register<C, D>((ns, cmd, data) => handler(cmd, data));

    public void Register<C, D>(Action<ReadOnlySpan<D>> handler)
        where C : unmanaged, ICommand where D : unmanaged =>
        Register<C, D>((NetSocket ns, ReadOnlySpan<D> data) => handler(data));
}
