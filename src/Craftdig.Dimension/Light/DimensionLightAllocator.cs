namespace Craftdig.Dimension;

[Dimension]
public class DimensionLightAllocator
{
    private readonly int pageBits = 8;
    private readonly int pageSize = 1 << 8;
    private readonly int pageMask = (1 << 8) - 1;

    private readonly List<byte[]> pages = [];
    private readonly ConcurrentBag<int> free = [];
    private int next;

    public int NibbleVolume => SectionVolume / 2;

    public int Alloc(byte uniform)
    {
        if (!free.TryTake(out var index))
            index = Interlocked.Increment(ref next);

        if (PageIndex(index) >= pages.Count)
        {
            lock (this)
            {
                if (PageIndex(index) >= pages.Count)
                    pages.Add(new byte[pageSize * NibbleVolume]);
            }
        }

        var memory = Memory(index);
        memory.Span.Fill((byte)(uniform | (uniform << 4)));
        return index;
    }

    public void Free(int index)
    {
        if (index != 0)
            free.Add(index);
    }

    public Memory<byte> Memory(int index) =>
        pages[PageIndex(index)]
            .AsMemory(SubIndex(index) * NibbleVolume, NibbleVolume);

    private int PageIndex(int index) => index >> pageBits;
    private int SubIndex(int index) => index & pageMask;
}
