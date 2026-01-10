namespace Craftdig.Dimension;

[Dimension]
public class DimensionBlocksAllocator
{
    private const int PageBits = 8;
    private const int PageSize = 1 << PageBits;
    private const int PageMask = PageSize - 1;

    private readonly List<Ent[]> pages = [];
    private readonly ConcurrentBag<int> free = [];
    private int next;

    public int Alloc()
    {
        if (free.TryTake(out var index))
            return index;

        index = Interlocked.Increment(ref next);

        if (PageIndex(index) >= pages.Count)
        {
            lock (this)
            {
                if (PageIndex(index) >= pages.Count)
                    pages.Add(new Ent[PageSize * SectionVolume]);
            }
        }

        return index;
    }

    public void Free(int index) => free.Add(index);

    public Memory<Ent> Memory(int index) =>
        pages[PageIndex(index)].AsMemory().Slice(SubIndex(index) * SectionVolume, SectionVolume);

    private int PageIndex(int index) => index >> PageBits;
    private int SubIndex(int index) => index & PageMask;
}
