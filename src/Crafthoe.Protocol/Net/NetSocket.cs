namespace Crafthoe.Protocol;

public class NetSocket(AppLog log, TcpClient tcp, Stream stream)
{
    private readonly EntObj ent = new();
    private byte[] buffer = [];

    public EntMut Ent => (EntMut)ent;
    public bool Connected => tcp.Connected;
    public EndPoint? Ip => tcp.Client.RemoteEndPoint;

    public bool TryGet(out NetMessage msg)
    {
        msg = default;

        log.Trace("Socket {0} reading type", ent.Tag());
        Span<byte> tb = stackalloc byte[2];
        if (!Read(tb))
            return false;

        ushort type = BinaryPrimitives.ReadUInt16BigEndian(tb);
        if (type <= 0)
            throw new Exception("Message type is invalid");

        log.Trace("Socket {0} read type : {1}", ent.Tag(), type);
        log.Trace("Socket {0} reading size", ent.Tag(), type);

        Span<byte> sb = stackalloc byte[4];
        if (!Read(sb))
            return false;

        int size = BinaryPrimitives.ReadInt32BigEndian(sb);
        if (size < 0)
            throw new Exception("Message size is invalid");

        if (buffer.Length < size)
            Array.Resize(ref buffer, (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)size));

        log.Trace("Socket {0} read size : {1}", ent.Tag(), size);
        log.Trace("Socket {0} reading data", ent.Tag());

        var data = buffer.AsSpan()[..size];
        if (!Read(data))
            return false;

        log.Trace("Socket {0} read data : {1} bytes", ent.Tag(), data.Length);

        msg = new(type, data);
        return true;
    }

    public void Send(ushort type, ReadOnlySpan<byte> cmd, ReadOnlySpan<byte> data)
    {
        lock (this)
        {
            Span<byte> tb = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(tb, type);

            Span<byte> sb = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(sb, cmd.Length + data.Length);

            try
            {
                stream.Write(tb);
                stream.Write(sb);
                stream.Write(cmd);
                stream.Write(data);
            }
            catch
            {
                log.Trace("Socket {0} unable to send ({1}) {2} bytes", ent.Tag(), type, data.Length);
            }
        }
    }

    public void Send<C, D>(C cmd, ReadOnlySpan<D> data)
        where C : unmanaged, ICommand where D : unmanaged
    {
        var cmdBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref cmd, 1));
        var dataBytes = MemoryMarshal.AsBytes(data);
        var bytes = cmdBytes.Length + dataBytes.Length + sizeof(ushort) + sizeof(int);
        log.Trace("Socket {0} -> {1} ({2}) {3} bytes", ent.Tag(), typeof(C).Name, C.CommandId, bytes);

        Send(C.CommandId, cmdBytes, dataBytes);
    }

    public void Send<C, D>(in C cmd, Span<D> data)
        where C : unmanaged, ICommand where D : unmanaged =>
        Send(cmd, (ReadOnlySpan<D>)data);

    public void Send<C>(in C cmd)
        where C : unmanaged, ICommand =>
        Send<C, byte>(cmd, []);

    public void Send<C, D>(ReadOnlySpan<D> data)
        where C : unmanaged, ICommand where D : unmanaged =>
        Send<C, D>(default, data);

    public void Send<C, D>(Span<D> data)
        where C : unmanaged, ICommand where D : unmanaged =>
        Send<C, D>(default, (ReadOnlySpan<D>)data);

    public void Send<C>()
        where C : unmanaged, ICommand =>
        Send<C, byte>(default, []);

    private bool Read(Span<byte> dst)
    {
        int r = 0;
        while (r < dst.Length)
        {
            int n = stream.Read(dst[r..]);
            if (n == 0)
                return false;
            r += n;
        }
        return true;
    }

    public void Disconnect()
    {
        try { stream.Dispose(); } catch { }
        try { tcp.Dispose(); } catch { }
    }
}
