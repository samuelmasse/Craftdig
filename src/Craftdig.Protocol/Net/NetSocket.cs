namespace Craftdig;

public class NetSocket(Log log, TcpClient tcp, Stream stream) : IEntMut
{
    private readonly EntPtr ent = new();
    private byte[] buffer = [];
    private long maxMessageSize = ProtocolLimits.MaxMessageSize;
    private TransportState transport;
    private OutputState output = new()
    {
        Semaphore = new(0),
        Segments =
        [
            new byte[ProtocolLimits.SegmentSize],
            new byte[ProtocolLimits.SegmentSize],
            new byte[ProtocolLimits.SegmentSize],
            new byte[ProtocolLimits.SegmentSize],
        ],
        CommitIndex = [0, 0, 0, 0],
        SendCommitIndex = [0, 0, 0, 0],
    };

    public bool Connected => Volatile.Read(ref transport.Disconnected) == 0 && tcp.Connected;
    public EndPoint? Ip => tcp.Client.RemoteEndPoint;
    public EntHandle Handle => ent.Handle;
    public bool IsAlive => ent.IsAlive;
    public ref long MaxMessageSize => ref maxMessageSize;
    public ref bool IsTransportSecure => ref transport.IsSecure;

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
                output.Semaphore.Wait(ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            while (Connected)
            {
                long rindex = output.SendIndex % output.Segments.Length;
                var segment = output.Segments[rindex];
                var commitIndex = output.CommitIndex[rindex];
                var sendCommitIndex = output.SendCommitIndex[rindex];

                if (sendCommitIndex < commitIndex)
                {
                    var data = segment.AsSpan()[sendCommitIndex..commitIndex];
                    stream.Write(data);
                    output.SendCommitIndex[rindex] = commitIndex;
                }
                else if (output.SegmentIndex > output.SendIndex)
                {
                    output.CommitIndex[rindex] = 0;
                    output.SendCommitIndex[rindex] = 0;
                    output.SendIndex++;
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

            var segment = output.Segments[output.SegmentIndex % output.Segments.Length];
            var commitIndex = output.CommitIndex[output.SegmentIndex % output.Segments.Length];
            int available = segment.Length - commitIndex;

            if (available < needed)
            {
                long nextSegmentIndex = output.SegmentIndex + 1;
                segment = output.Segments[nextSegmentIndex % output.Segments.Length];
                commitIndex = output.CommitIndex[nextSegmentIndex % output.Segments.Length];

                if (needed > segment.Length || commitIndex != 0)
                {
                    log.Trace("Socket {0} unable to send ({1}) {2} bytes", ent.Tag, type, needed);
                    return false;
                }

                output.SegmentIndex = nextSegmentIndex;
            }

            Write(tb);
            Write(sb);
            Write(cmd);
            Write(data);
            output.CommitIndex[output.SegmentIndex % output.Segments.Length] += needed;
            output.Semaphore.Release();
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
        lock (this)
        {
            if (transport.Disconnected != 0)
                return;

            Volatile.Write(ref transport.Disconnected, 1);
            stream.Dispose();
            tcp.Dispose();
            output.Semaphore.Release();
        }
    }

    public bool Has<T, N>() => ent.Has<T, N>();
    public T? Get<T, N>() => ent.Get<T, N>();
    public void Set<T, N>(in T value) => ent.Set<T, N>(value);
    public bool Unset<T, N>() => ent.Unset<T, N>();

    // The owner calls this only after I/O and every metadata consumer have finished.
    public void ReleaseState()
    {
        lock (this)
        {
            ent.Dispose();
            output.Semaphore.Dispose();
        }
    }

    private struct TransportState
    {
        public int Disconnected;
        public bool IsSecure;
    }

    private struct OutputState
    {
        public SemaphoreSlim Semaphore;
        public byte[][] Segments;
        public int[] CommitIndex;
        public int[] SendCommitIndex;
        public long SegmentIndex;
        public long SendIndex;
    }
}
