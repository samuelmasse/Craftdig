namespace Craftdig.Protocol;

public class NetSocket(AppLog log, TcpClient tcp, Stream stream) : IEntMut
{
    private readonly EntObj ent = new();
    private byte[] buffer = [];
    private long maxMessageSize = ProtocolLimits.MaxMessageSize;

    private readonly SemaphoreSlim outSemaphore = new(0);
    private readonly byte[][] outSegments =
    [
        new byte[ProtocolLimits.SegmentSize],
        new byte[ProtocolLimits.SegmentSize],
        new byte[ProtocolLimits.SegmentSize],
        new byte[ProtocolLimits.SegmentSize]
    ];
    private readonly int[] outSegmentCommitIndex = [0, 0, 0, 0];
    private readonly int[] outSegmentSendCommitIndex = [0, 0, 0, 0];
    private long outSegmentIndex;
    private long outSegmentSendIndex;

    public bool Connected => tcp.Connected;
    public bool IsTransportSecure;
    public EndPoint? Ip => tcp.Client.RemoteEndPoint;
    public ref long MaxMessageSize => ref maxMessageSize;

    public bool TryGet(out NetMessage msg)
    {
        msg = default;

        Span<byte> tb = stackalloc byte[2];
        if (!Read(tb))
            return false;

        ushort type = BinaryPrimitives.ReadUInt16BigEndian(tb);
        if (type <= 0)
            throw new Exception($"Message type is invalid : {type}");

        Span<byte> sb = stackalloc byte[4];
        if (!Read(sb))
            return false;

        int size = BinaryPrimitives.ReadInt32BigEndian(sb);
        if (size < 0 || size > maxMessageSize)
            throw new Exception($"Message size is invalid : {size}");

        if (buffer.Length < size)
            Array.Resize(ref buffer, (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)size));

        var data = buffer.AsSpan()[..size];
        if (!Read(data))
            return false;

        msg = new(type, data);
        return true;
    }

    public void Push(CancellationToken ct)
    {
        while (Connected)
        {
            try
            {
                outSemaphore.Wait(ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            while (Connected)
            {
                long rindex = outSegmentSendIndex % outSegments.Length;
                var segment = outSegments[rindex];
                var commitIndex = outSegmentCommitIndex[rindex];
                var sendCommitIndex = outSegmentSendCommitIndex[rindex];

                if (sendCommitIndex < commitIndex)
                {
                    var data = segment.AsSpan()[sendCommitIndex..commitIndex];
                    stream.Write(data);
                    outSegmentSendCommitIndex[rindex] = commitIndex;
                }
                else if (outSegmentIndex > outSegmentSendIndex)
                {
                    outSegmentCommitIndex[rindex] = 0;
                    outSegmentSendCommitIndex[rindex] = 0;
                    outSegmentSendIndex++;
                }
                else break;
            }
        }
    }

    public bool TrySend(ushort type, ReadOnlySpan<byte> cmd, ReadOnlySpan<byte> data)
    {
        lock (this)
        {
            Span<byte> tb = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(tb, type);

            Span<byte> sb = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(sb, cmd.Length + data.Length);

            int needed = tb.Length + sb.Length + cmd.Length + data.Length;

            var segment = outSegments[outSegmentIndex % outSegments.Length];
            var commitIndex = outSegmentCommitIndex[outSegmentIndex % outSegments.Length];
            int available = segment.Length - commitIndex;

            if (available < needed)
            {
                long nextSegmentIndex = outSegmentIndex + 1;
                segment = outSegments[nextSegmentIndex % outSegments.Length];
                commitIndex = outSegmentCommitIndex[nextSegmentIndex % outSegments.Length];

                if (needed > segment.Length || commitIndex != 0)
                {
                    log.Trace("Socket {0} unable to send ({1}) {2} bytes", ent.Tag, type, needed);
                    return false;
                }

                outSegmentIndex = nextSegmentIndex;
            }

            Write(tb);
            Write(sb);
            Write(cmd);
            Write(data);
            outSegmentCommitIndex[outSegmentIndex % outSegments.Length] += needed;
            outSemaphore.Release();
            return true;

            void Write(ReadOnlySpan<byte> bytes)
            {
                bytes.CopyTo(segment.AsSpan()[commitIndex..]);
                commitIndex += bytes.Length;
            }
        }
    }

    public void Send(ushort type, ReadOnlySpan<byte> cmd, ReadOnlySpan<byte> data)
    {
        if (!TrySend(type, cmd, data))
            Disconnect();
    }

    public void Send<C, D>(C cmd, ReadOnlySpan<D> data)
        where C : unmanaged, ICommand where D : unmanaged
    {
        var cmdBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref cmd, 1));
        var dataBytes = MemoryMarshal.AsBytes(data);
        var bytes = cmdBytes.Length + dataBytes.Length + sizeof(ushort) + sizeof(int);
        log.Trace("Socket {0} -> {1} ({2}) {3} bytes", ent.Tag, typeof(C).Name, C.CommandId, bytes);

        Send(C.CommandId, cmdBytes, dataBytes);
    }

    public void SendRaw<C>(ReadOnlySpan<byte> body)
        where C : ICommand
    {
        if (body.Length > ProtocolLimits.MaxMessageSize)
            throw new ArgumentOutOfRangeException(nameof(body), body.Length, "The raw command body exceeds the protocol message limit.");

        var bytes = body.Length + sizeof(ushort) + sizeof(int);
        log.Trace("Socket {0} -> {1} ({2}) {3} bytes", ent.Tag, typeof(C).Name, C.CommandId, bytes);
        Send(C.CommandId, body, []);
    }

    public bool TrySendRaw<C>(ReadOnlySpan<byte> body)
        where C : ICommand
    {
        if (body.Length > ProtocolLimits.MaxMessageSize)
            throw new ArgumentOutOfRangeException(nameof(body), body.Length, "The raw command body exceeds the protocol message limit.");

        var bytes = body.Length + sizeof(ushort) + sizeof(int);
        log.Trace("Socket {0} -> {1} ({2}) {3} bytes", ent.Tag, typeof(C).Name, C.CommandId, bytes);
        return TrySend(C.CommandId, body, []);
    }

    public bool TrySend<C>()
        where C : unmanaged, ICommand
    {
        C command = default;
        var commandBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref command, 1));
        return TrySend(C.CommandId, commandBytes, []);
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
        outSemaphore.Release(ushort.MaxValue);
    }

    public EntHandle Handle => ent.Handle;
    public bool IsAlive => ent.IsAlive;
    public bool Has<T, N>() => ent.Has<T, N>();
    public T? Get<T, N>() => ent.Get<T, N>();
    public void Set<T, N>(in T value) => ent.Set<T, N>(value);
    public bool Unset<T, N>() => ent.Unset<T, N>();
}
