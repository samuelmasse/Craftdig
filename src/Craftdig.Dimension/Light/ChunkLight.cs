namespace Craftdig.Dimension;

public sealed class ChunkLight(DimensionLightAllocator allocator)
{
    private readonly SectionLight[] sections = new SectionLight[SectionHeight];
    private readonly ushort[] skyHeight = new ushort[SectionSize * SectionSize];

    public Span<ushort> SkyHeight => skyHeight;
    public ReadOnlySpan<ushort> SkyHeightReadOnly => skyHeight;

    public byte Sky(Vec3i loc)
    {
        if ((uint)loc.Z >= HeightSize)
            return 0;

        if (loc.Z >= skyHeight[ColumnIndex(loc)])
            return LightLevel.Max;

        return Stored(LightChannel.Sky, loc);
    }

    public byte Block(Vec3i loc) => (uint)loc.Z < HeightSize ? Stored(LightChannel.Block, loc) : (byte)0;

    public byte Stored(LightChannel channel, Vec3i loc)
    {
        ref var section = ref sections[loc.Z >> SectionBits];
        var data = channel == LightChannel.Sky ? section.SkyData.Span : section.BlockData.Span;
        byte uniform = channel == LightChannel.Sky ? section.SkyUniform : section.BlockUniform;
        if (data.IsEmpty)
            return uniform;

        int index = InnerIndex(loc);
        byte packed = data[index >> 1];
        return (byte)((packed >> ((index & 1) << 2)) & LightLevel.Max);
    }

    public bool SetStored(LightChannel channel, Vec3i loc, byte value)
    {
        ref var section = ref sections[loc.Z >> SectionBits];
        ref var uniform = ref channel == LightChannel.Sky ? ref section.SkyUniform : ref section.BlockUniform;
        ref var data = ref channel == LightChannel.Sky ? ref section.SkyData : ref section.BlockData;
        ref var alloc = ref channel == LightChannel.Sky ? ref section.SkyAlloc : ref section.BlockAlloc;

        if (data.IsEmpty)
        {
            if (uniform == value)
                return false;

            alloc = allocator.Alloc(uniform);
            data = allocator.Memory(alloc);
        }

        int index = InnerIndex(loc);
        ref byte packed = ref data.Span[index >> 1];
        int shift = (index & 1) << 2;
        byte previous = (byte)((packed >> shift) & LightLevel.Max);
        if (previous == value)
            return false;

        packed = (byte)((packed & ~(LightLevel.Max << shift)) | (value << shift));
        return true;
    }

    public void Clear()
    {
        for (int i = 0; i < sections.Length; i++)
        {
            ref var section = ref sections[i];
            allocator.Free(section.SkyAlloc);
            allocator.Free(section.BlockAlloc);
            section = default;
        }

        skyHeight.AsSpan().Clear();
    }

    public void CopySection(LightChannel channel, int sz, Span<byte> destination)
    {
        ref var section = ref sections[sz];
        var source = channel == LightChannel.Sky ? section.SkyData.Span : section.BlockData.Span;
        byte uniform = channel == LightChannel.Sky ? section.SkyUniform : section.BlockUniform;

        if (source.IsEmpty)
            destination[..allocator.NibbleVolume].Fill((byte)(uniform | (uniform << 4)));
        else source.CopyTo(destination);
    }

    public void LoadSection(LightChannel channel, int sz, ReadOnlySpan<byte> source)
    {
        ref var section = ref sections[sz];
        ref var uniform = ref channel == LightChannel.Sky ? ref section.SkyUniform : ref section.BlockUniform;
        ref var data = ref channel == LightChannel.Sky ? ref section.SkyData : ref section.BlockData;
        ref var alloc = ref channel == LightChannel.Sky ? ref section.SkyAlloc : ref section.BlockAlloc;

        allocator.Free(alloc);
        alloc = 0;
        data = default;

        source = source[..allocator.NibbleVolume];
        if (TryGetUniform(source, out uniform))
            return;

        uniform = 0;
        alloc = allocator.Alloc(0);
        data = allocator.Memory(alloc);
        source.CopyTo(data.Span);
    }

    public void CopyFrom(ChunkLight source, uint sectionMask)
    {
        source.SkyHeightReadOnly.CopyTo(skyHeight);

        Span<byte> buffer = stackalloc byte[allocator.NibbleVolume];
        for (int sz = 0; sz < SectionHeight; sz++)
        {
            if ((sectionMask & (1u << sz)) == 0)
                continue;

            source.CopySection(LightChannel.Sky, sz, buffer);
            LoadSection(LightChannel.Sky, sz, buffer);
            source.CopySection(LightChannel.Block, sz, buffer);
            LoadSection(LightChannel.Block, sz, buffer);
        }
    }

    public bool IsDirectSky(Vec3i loc) =>
        (uint)loc.Z < HeightSize && loc.Z >= skyHeight[ColumnIndex(loc)];

    public bool MayHaveStoredLight(LightChannel channel, int sz)
    {
        ref var section = ref sections[sz];
        var data = channel == LightChannel.Sky ? section.SkyData.Span : section.BlockData.Span;
        byte uniform = channel == LightChannel.Sky ? section.SkyUniform : section.BlockUniform;
        return !data.IsEmpty || uniform != 0;
    }

    private bool TryGetUniform(ReadOnlySpan<byte> data, out byte uniform)
    {
        byte packed = data[0];
        if ((packed & LightLevel.Max) != (packed >> 4))
        {
            uniform = 0;
            return false;
        }

        foreach (byte item in data)
        {
            if (item != packed)
            {
                uniform = 0;
                return false;
            }
        }

        uniform = (byte)(packed & LightLevel.Max);
        return true;
    }

    private int ColumnIndex(Vec3i loc) =>
        ((loc.Y & SectionMask) << SectionBits) | (loc.X & SectionMask);

    private int InnerIndex(Vec3i loc) =>
        ((loc.Z & SectionMask) << (SectionBits * 2)) |
        ((loc.Y & SectionMask) << SectionBits) |
        (loc.X & SectionMask);
}
