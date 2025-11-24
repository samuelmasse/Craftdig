namespace Crafthoe.App;

public class LogBuffer
{
    private static readonly ArrayPool<LogBufferEntry> EntriesPool = ArrayPool<LogBufferEntry>.Create();
    private static readonly ArrayPool<char> CharsPool = ArrayPool<char>.Create();

    private readonly LogBufferEntry[] entries = EntriesPool.Rent(0xFFF);
    private readonly char[] chars = CharsPool.Rent(0xFFFF);

    private int charWritten;
    private int written;
    private int read;

    public int CharCapacity => chars.Length - charWritten;
    public int Capacity => entries.Length - written;
    public bool Synced => written == read;

    public void Write(LogEntry entry, ReadOnlySpan<char> text)
    {
        var dst = new Memory<char>(chars, charWritten, text.Length);
        text.CopyTo(dst.Span);

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

    public void Return()
    {
        EntriesPool.Return(entries);
        CharsPool.Return(chars);
    }
}
