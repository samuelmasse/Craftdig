namespace Craftdig.Protocol;

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

    public void RegisterRaw<C>(RawCommandHandler handler)
        where C : ICommand =>
        Register(C.CommandId, (ns, msg) =>
        {
            log.Trace("Socket {0} <- {1} ({2}) {3} bytes", ns.Tag, typeof(C).Name, C.CommandId, msg.Data.Length);
            handler(ns, msg.Data);
        });

    public void RegisterGuarded<C>(RawCommandValidator validate, RawCommandHandler handler)
        where C : ICommand =>
        RegisterRaw<C>((ns, data) =>
        {
            if (!validate(data))
            {
                DisconnectMalformed<C>(ns, data.Length);
                return;
            }

            handler(ns, data);
        });

    public void RegisterDecoded<C, V>(CommandDecoder<V> decoder, Action<NetSocket, V> handler)
        where C : ICommand =>
        RegisterRaw<C>((ns, data) =>
        {
            if (!decoder(data, out var value))
            {
                DisconnectMalformed<C>(ns, data.Length);
                return;
            }

            handler(ns, value);
        });

    private void DisconnectMalformed<C>(NetSocket ns, int bytes) where C : ICommand
    {
        log.Warn("Socket {0} sent a malformed {1} frame ({2} bytes); disconnecting", ns.Tag, typeof(C).Name, bytes);
        ns.Disconnect();
    }

    public void Register<C, D>(Action<NetSocket, C, ReadOnlySpan<D>> handler)
        where C : unmanaged, ICommand where D : unmanaged
    {
        int header = Unsafe.SizeOf<C>();

        Register(C.CommandId, (ns, msg) =>
        {
            if (msg.Data.Length < header)
                throw new InvalidDataException($"Message {C.CommandId} is shorter than its command header.");

            log.Trace("Socket {0} <- {1} ({2}) {3} bytes", ns.Tag, typeof(C).Name, C.CommandId, msg.Data.Length);

            var cmd = MemoryMarshal.AsRef<C>(msg.Data[..header]);
            var data = msg.Data[header..];
            if (data.Length % Unsafe.SizeOf<D>() != 0)
                throw new InvalidDataException($"Message {C.CommandId} has an incomplete payload element.");
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
