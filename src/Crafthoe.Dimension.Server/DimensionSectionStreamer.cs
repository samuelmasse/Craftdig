namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionSectionStreamer(WorldModuleIndices moduleIndices)
{
    private readonly EntCompressor compressor = new(moduleIndices, SectionVolume);

    public Span<byte> Command(Vector3i sloc, ChunkBlocks blocks, out SectionUpdateCommand cmd)
    {
        cmd = new SectionUpdateCommand() { Sloc = sloc };
        return compressor.Compress(blocks.Slice(sloc.Z));
    }
}
