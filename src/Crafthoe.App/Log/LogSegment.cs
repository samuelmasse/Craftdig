namespace Craftdig.App;

public class LogSegment
{
    private readonly LogBufferEntry[] entries = new LogBufferEntry[4096];
    private readonly char[] chars = new char[65536];
    private int entryIndex;
    private int charIndex;
    private bool closed;

    public bool Closed => closed;
    public int Capacity => entries.Length - entryIndex;
    public int CharCapacity => chars.Length - charIndex;
    public ReadOnlySpan<LogBufferEntry> Entries => new(entries, 0, entryIndex);

    public void Add(LogBufferEntry entry)
    {
        var dst = new Memory<char>(chars, charIndex, entry.Chars.Length);
        entry.Chars.CopyTo(dst);

        entries[entryIndex] = new(entry.Entry, dst);

        charIndex += entry.Chars.Length;
        entryIndex++;
    }

    public void Reset()
    {
        lock (this)
        {
            entryIndex = 0;
            charIndex = 0;
            closed = false;
        }
    }

    public void Close() => closed = true;
}
