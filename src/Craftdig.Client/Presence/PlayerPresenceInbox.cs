namespace Craftdig.Client;

internal sealed class PlayerPresenceInbox(int maxMessages, int maxBytes)
{
    private readonly Lock gate = new();
    private readonly AutoResetEvent workReady = new(false);
    private readonly Queue<byte[]>[] lanes = [[], [], []];
    private int queuedBytes;
    private int accepting;

    public void Open() => Volatile.Write(ref accepting, 1);

    public void Close()
    {
        Volatile.Write(ref accepting, 0);
        lock (gate)
        {
            foreach (var lane in lanes)
                lane.Clear();
            queuedBytes = 0;
        }

        workReady.Set();
    }

    public void Enqueue(PlayerPresenceLane lane, ReadOnlySpan<byte> data)
    {
        if (Volatile.Read(ref accepting) == 0)
        {
            return;
        }

        lock (gate)
        {
            if (Volatile.Read(ref accepting) == 0)
            {
                return;
            }

            int messageCount = 0;
            foreach (var queue in lanes)
                messageCount += queue.Count;
            if (messageCount >= maxMessages || queuedBytes + data.Length > maxBytes)
            {
                return;
            }

            lanes[(int)lane].Enqueue(data.ToArray());
            queuedBytes += data.Length;
        }

        workReady.Set();
    }

    public bool TryDequeue(PlayerPresenceLane lane, [NotNullWhen(true)] out byte[]? body)
    {
        lock (gate)
        {
            if (!lanes[(int)lane].TryDequeue(out body))
                return false;

            queuedBytes -= body.Length;
            return true;
        }
    }

    public void SignalWork() => workReady.Set();

    public void WaitForWork(TimeSpan timeout) => workReady.WaitOne(timeout);
}
