namespace Craftdig.Dimension;

internal struct SectionLight
{
    public byte SkyUniform;
    public Memory<byte> SkyData;
    public int SkyAlloc;

    public byte BlockUniform;
    public Memory<byte> BlockData;
    public int BlockAlloc;
}
