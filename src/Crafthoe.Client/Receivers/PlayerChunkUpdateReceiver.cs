namespace Crafthoe.Client;

[Player]
public class PlayerChunkUpdateReceiver(
    WorldModuleIndices moduleIndices,
    DimensionBlocksPool blocksPool,
    PlayerChunkUpdateQueue chunkUpdateQueue)
{
    private readonly EntDecompressor decompressor = new(moduleIndices, ChunkVolume);

    public void Receive(ChunkUpdateCommand cmd, ReadOnlySpan<byte> data)
    {
        var blocks = blocksPool.Take();
        decompressor.Decompress(data, blocks.Span);
        chunkUpdateQueue.Enqueue((cmd.Cloc, blocks));
    }
}
