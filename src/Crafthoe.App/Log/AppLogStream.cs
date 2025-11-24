namespace Crafthoe.App;

[App]
public class AppLogStream
{
    private readonly List<LogThread> logs;
    private readonly ThreadLocal<LogThread> localLogThread;
    private readonly List<LogBufferEntry> aggregateEntries = [];
    private readonly List<DateTime> aggregateTimes = [];
    private readonly List<(LogThread, LogBuffer, int)> aggregateReads = [];
    private readonly LogSegment[] segments;
    private long segmentIndex;
    private long logCount;

    public ReadOnlySpan<LogSegment> Segments => segments;
    public long SegmentIndex => segmentIndex;
    public long LogCount => logCount;

    public AppLogStream()
    {
        logs = [];
        localLogThread = new(CreateLogThreads);

        segments = new LogSegment[0xF];
        for (int i = 0; i < segments.Length; i++)
            segments[i] = new();
    }

    public void Add(LogEntry entry, ReadOnlySpan<char> chars) => localLogThread.Value!.Add(entry, chars);

    public void Collect(double ms = 5)
    {
        CollectEntries(ms);
        SortEntries();
        CommitEntries();
        AdvanceBuffers();
        CleanupLogThreads();
    }

    private void CollectEntries(double ms)
    {
        aggregateEntries.Clear();
        aggregateTimes.Clear();
        aggregateReads.Clear();

        var now = DateTime.UtcNow;

        foreach (var log in CollectionsMarshal.AsSpan(logs))
        {
            foreach (var buffer in log.Buffers)
            {
                var entries = buffer.Read();
                if (entries.Length == 0)
                    continue;

                int count = 0;
                while (count < entries.Length && (now - entries[count].Entry.Time).TotalMilliseconds > ms)
                {
                    aggregateEntries.Add(entries[count]);
                    aggregateTimes.Add(entries[count].Entry.Time);
                    count++;
                }

                aggregateReads.Add((log, buffer, count));
            }
        }
    }

    private void SortEntries()
    {
        var spanEntries = CollectionsMarshal.AsSpan(aggregateEntries);
        var spanTimes = CollectionsMarshal.AsSpan(aggregateTimes);
        spanTimes.Sort(spanEntries);
    }

    private void CommitEntries()
    {
        foreach (var entry in aggregateEntries)
        {
            var segment = segments[segmentIndex % segments.Length];
            if (segment.Capacity == 0 || segment.CharCapacity <= entry.Chars.Length)
            {
                segment.Close();
                segment = segments[(segmentIndex + 1) % segments.Length];
                segment.Reset();
                segmentIndex++;
            }

            segment.Add(entry);
            logCount++;
        }
    }

    private void AdvanceBuffers()
    {
        foreach (var (log, buffer, count) in aggregateReads)
        {
            buffer.Advance(count);

            lock (log)
            {
                System.Threading.Monitor.Pulse(log);
            }
        }
    }

    private void CleanupLogThreads()
    {
        lock (this)
        {
            for (int i = logs.Count - 1; i >= 0; i--)
            {
                if (logs[i].Thread.IsAlive)
                    continue;

                bool synced = true;
                foreach (var buffer in logs[i].Buffers)
                {
                    if (!buffer.Synced)
                    {
                        synced = false;
                        break;
                    }
                }

                if (synced)
                {
                    foreach (var buffer in logs[i].Buffers)
                        buffer.Return();

                    logs.RemoveAt(i);
                }
            }
        }
    }

    private LogThread CreateLogThreads()
    {
        lock (this)
        {
            var log = new LogThread(Thread.CurrentThread);
            logs.Add(log);
            return log;
        }
    }
}
