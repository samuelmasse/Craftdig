namespace Craftdig;

[Dimension]
public class DimensionChunkLightFormat(DimensionLightAllocator allocator)
{
    public uint AllSectionsMask => uint.MaxValue;
    public int HeightBytes => SectionSize * SectionSize * sizeof(ushort);
    public int SectionBytes => allocator.NibbleVolume * 2;
    public int MaximumBytes => HeightBytes + SectionHeight * SectionBytes;
}
