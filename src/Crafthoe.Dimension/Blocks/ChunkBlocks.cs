namespace Crafthoe.Dimension;

public class ChunkBlocks
{
    private readonly static EntPtr Empty = new();

    private readonly DimensionBlocksAllocator allocator;
    private readonly SectionBlocks[] sections = new SectionBlocks[SectionHeight];

    public Ent this[Vector3i index]
    {
        get
        {
            int sz = index.Z >> SectionBits;
            var section = sections[sz];
            var span = section.Data.Span;

            if (span.IsEmpty)
                return section.Uniform;

            return span[InnerIndex(index)];
        }
        set
        {
            int sz = index.Z >> SectionBits;

            Unpack(sz);
            sections[sz].Data.Span[InnerIndex(index)] = value;
        }
    }

    public ChunkBlocks(DimensionBlocksAllocator allocator)
    {
        this.allocator = allocator;

        for (int i = 0; i < sections.Length; i++)
            Fill(i, Empty);
    }

    public Span<Ent> Slice(int sz)
    {
        Unpack(sz);
        return sections[sz].Data.Span;
    }

    public Ent Uniform(int sz) => sections[sz].Uniform;

    public void Fill(Ent uniform)
    {
        for (int i = 0; i < sections.Length; i++)
            Fill(i, uniform);
    }

    public void Fill(int sz, Ent uniform)
    {
        ref var section = ref sections[sz];
        section.Uniform = uniform;

        if (section.Alloc != 0)
        {
            allocator.Free(section.Alloc);
            section.Data = default;
            section.Alloc = default;
        }
    }

    public bool Pack(int sz)
    {
        ref var section = ref sections[sz];
        if (section.Uniform != default)
            return false;

        var span = section.Data.Span;
        var same = span[0];
        foreach (var item in span)
        {
            if (item != same)
                return false;
        }

        Fill(sz, section.Uniform);
        return true;
    }

    public void Unpack(int sz)
    {
        ref var section = ref sections[sz];
        if (section.Uniform == default)
            return;

        var alloc = allocator.Alloc();
        var data = allocator.Memory(alloc);

        data.Span.Fill(section.Uniform);

        section.Data = data;
        section.Alloc = alloc;
        section.Uniform = default;
    }

    private int InnerIndex(Vector3i index) =>
        ((index.Z & SectionMask) << (SectionBits * 2)) + ((index.Y & SectionMask) << SectionBits) + (index.X & SectionMask);
}
