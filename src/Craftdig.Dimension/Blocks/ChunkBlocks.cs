namespace Craftdig;

public class ChunkBlocks(DimensionBlocksAllocator allocator)
{
    private readonly SectionBlocks[] sections = new SectionBlocks[SectionHeight];

    public Ent this[Vec3i index]
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

    public Span<Ent> Slice(int sz)
    {
        Unpack(sz);
        return sections[sz].Data.Span;
    }

    public bool TryGetUniform(int sz, out Ent uniform)
    {
        ref var section = ref sections[sz];
        uniform = section.Uniform;
        return section.Data.IsEmpty;
    }

    public ReadOnlySpan<Ent> ReadSection(int sz, out Ent uniform)
    {
        var section = sections[sz];
        uniform = section.Uniform;
        return section.Data.Span;
    }

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
        if (section.Data.IsEmpty)
            return false;

        var span = section.Data.Span;
        var same = span[0];
        foreach (var item in span)
        {
            if (item != same)
                return false;
        }

        Fill(sz, same);
        return true;
    }

    public void Unpack(int sz)
    {
        ref var section = ref sections[sz];
        if (!section.Data.IsEmpty)
            return;

        var alloc = allocator.Alloc();
        var data = allocator.Memory(alloc);

        data.Span.Fill(section.Uniform);

        section.Data = data;
        section.Alloc = alloc;
        section.Uniform = default;
    }

    private int InnerIndex(Vec3i index) =>
        ((index.Z & SectionMask) << (SectionBits * 2)) + ((index.Y & SectionMask) << SectionBits) + (index.X & SectionMask);
}
