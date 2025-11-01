namespace Crafthoe.Protocol;

public class NetSocket(Socket socket)
{
    private readonly EntObj ent = new();
    private byte[] buffer = [];

    public EntMut Ent => (EntMut)ent;
    public Socket Raw => socket;

    public bool TryGet(out NetMessage msg)
    {
        msg = default;

        Span<byte> tb = stackalloc byte[4];
        if (!Read(tb))
            return false;

        int type = BinaryPrimitives.ReadInt32BigEndian(tb);
        if (type <= 0)
            throw new Exception("Message type is invalid");

        Span<byte> sb = stackalloc byte[4];
        if (!Read(sb))
            return false;

        int size = BinaryPrimitives.ReadInt32BigEndian(sb);
        if (size < 0)
            throw new Exception("Message size is invalid");

        if (buffer.Length < size)
            Array.Resize(ref buffer, (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)size));

        var data = buffer.AsSpan()[..size];
        if (!Read(data))
            return false;

        msg = new(type, data);
        return true;
    }

    public void Send(NetMessage msg)
    {
        Span<byte> tb = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(tb, msg.Type);

        Span<byte> sb = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(sb, msg.Data.Length);

        try
        {
            socket.Send(tb);
            socket.Send(sb);
            socket.Send(msg.Data);
        }
        catch { }
    }

    private bool Read(Span<byte> dst)
    {
        int r = 0;
        while (r < dst.Length)
        {
            int n = socket.Receive(dst[r..]);
            if (n == 0)
                return false;
            r += n;
        }
        return true;
    }
}
