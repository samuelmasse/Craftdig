namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionSectionStreamer(
    WorldModuleIndices moduleIndices,
    DimensionBlocksRaw blocksRaw)
{
    private readonly EntCompressor compressor = new(moduleIndices, SectionVolume);

    public Span<byte> Command(Vector3i sloc, out SectionUpdateCommand cmd)
    {
        cmd = new SectionUpdateCommand() { Sloc = sloc };
        return compressor.Compress(blocksRaw.Slice(sloc).Span);
    }
}
