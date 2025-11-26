namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionChunkStreamer(
    WorldModuleIndices moduleIndices,
    DimensionBlocksRaw blocksRaw)
{
    private readonly EntCompressor compressor = new(moduleIndices, ChunkVolume);

    public void Stream(NetSocket ns, Vector2i cloc)
    {
        var compressed = compressor.Compress(blocksRaw.Memory(cloc).Span);
        ns.Send(new ChunkUpdateCommand() { Cloc = cloc }, compressed);
    }
}
