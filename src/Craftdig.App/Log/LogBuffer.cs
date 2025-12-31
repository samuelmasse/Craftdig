namespace Craftdig.App;

public class LogBuffer
{
    private const int EntriesMax = 4096;
    private const int CharsMax = 65536;

    private LogBufferEntry[] entries = new LogBufferEntry[4];
    private char[] chars = new char[16];

    private int charWritten;
    private int written;
    private int read;

    public int CharCapacity => CharsMax - charWritten;
    public int Capacity => EntriesMax - written;
    public bool Synced => written == read;

    public void Write(LogEntry entry, ReadOnlySpan<char> text)
    {
        if (charWritten + text.Length >= chars.Length)
            Array.Resize(ref chars, (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)(charWritten + text.Length)));

        var dst = new Memory<char>(chars, charWritten, text.Length);
        text.CopyTo(dst.Span);

        if (written >= entries.Length)
            Array.Resize(ref entries, entries.Length * 2);

        entries[written] = new(entry, dst);

        charWritten += text.Length;
        written++;
    }

    public ReadOnlySpan<LogBufferEntry> Read()
    {
        int w = written;
        int start = read;
        return new ReadOnlySpan<LogBufferEntry>(entries, start, w - start);
    }

    public void Advance(int count)
    {
        read += count;
    }

    public void Clear()
    {
        charWritten = 0;
        written = 0;
        read = 0;
    }
}
