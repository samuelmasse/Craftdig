namespace Crafthoe.Client;

[Player]
public class PlayerChunkUpdateReceiver(
    WorldModuleIndices moduleIndices,
    DimensionBlocksPool blocksPool,
    PlayerChunkUpdateQueue chunkUpdateQueue)
{
    private readonly ChunkUpdateBlockEntry[] buffer = new ChunkUpdateBlockEntry[ChunkVolume];

    public void Receive(ChunkUpdateCommand cmd, ReadOnlySpan<byte> data)
    {
        var blocks = blocksPool.Take();

        BrotliDecoder.TryDecompress(
            data,
            MemoryMarshal.AsBytes(buffer.AsSpan()),
            out var bytes);

        int count = bytes / ChunkUpdateBlockEntry.Size;
        var entries = buffer.AsSpan()[..count];
        int cur = 0;

        foreach (var entry in entries)
        {
            var block = moduleIndices[entry.Value];

            for (int i = 0; i < entry.Count; i++)
                blocks.Span[cur++] = block;
        }

        chunkUpdateQueue.Enqueue((cmd.Cloc, blocks));
    }
}
