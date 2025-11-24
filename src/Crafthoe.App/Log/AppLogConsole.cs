namespace Crafthoe.App;

[App]
public class AppLogConsole(AppLogStream logStream)
{
    private readonly char[] newline = Environment.NewLine.ToCharArray();
    private readonly char[] buffer = new char[
        LogSegment.CharBufferSize + LogSegment.EntryBufferSize * 0xFF];

    private bool noColor;
    private int bufferIndex;
    private long segmentIndex;
    private int segmentInnerIndex;
    private long segmentDroppedCount;
    private long logCount;

    public ref bool NoColor => ref noColor;
    public long SegmentIndex => segmentIndex;
    public long SegmentDroppedCount => segmentDroppedCount;
    public long LogCount => logCount;

    public void Print()
    {
        while (true)
        {
            long nextIndex = logStream.SegmentIndex;
            long diff = nextIndex - segmentIndex;
            long maxDiff = logStream.Segments.Length / 2;
            long diffDiff = diff - maxDiff;

            if (diffDiff > 0)
            {
                diff -= diffDiff;
                segmentDroppedCount += diffDiff;
                segmentIndex += diffDiff;
                segmentInnerIndex = 0;
            }

            var segment = logStream.Segments[(int)(segmentIndex % logStream.Segments.Length)];

            lock (segment)
            {
                var closed = segment.Closed;
                var entries = segment.Entries[segmentInnerIndex..];

                if (diff == 0 && entries.Length == 0)
                    return;

                for (int i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i];

                    var stype = entry.Entry.Level switch
                    {
                        LogLevel.Fatal => "\x1b[1;37;41m",
                        LogLevel.Error => "\x1b[1;91;49m",
                        LogLevel.Warn => "\x1b[1;93;49m",
                        LogLevel.Info => "\x1b[1;37;49m",
                        LogLevel.Debug => "\x1b[0;37;49m",
                        LogLevel.Trace => "\x1b[0;90;49m",
                        _ => "\x1b[0m"
                    };

                    int start = 0;
                    int length = 0;

                    for (int j = 0; j < entry.Chars.Length; j++)
                    {
                        var c = entry.Chars.Span[j];
                        if (c == '\n')
                        {
                            Flush();
                            start = j + 1;
                            length = 0;
                        }
                        else if (c != '\r')
                            length++;
                    }

                    Flush();

                    void Flush()
                    {
                        if (length > 0)
                        {
                            if (!noColor)
                                Write(stype);
                            Write(entry.Chars.Span.Slice(start, length));
                            if (!noColor)
                                Write("\x1b[0m");
                        }

                        Write(newline);
                    }
                }

                logCount += entries.Length;
                segmentInnerIndex += entries.Length;

                if (closed)
                {
                    segmentIndex++;
                    segmentInnerIndex = 0;
                }
            }

            if (bufferIndex > 0)
            {
                Console.Out.Write(buffer.AsSpan()[..bufferIndex]);
                bufferIndex = 0;
            }
        }
    }

    private void Write(ReadOnlySpan<char> text)
    {
        text.CopyTo(new(buffer, bufferIndex, text.Length));
        bufferIndex += text.Length;
    }
}
