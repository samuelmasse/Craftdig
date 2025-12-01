namespace Crafthoe.Client;

[Player]
public class PlayerChunkUpdateReceiver(
    WorldModuleIndices moduleIndices,
    DimensionBlocksAllocator blocksAllocator,
    PlayerChunkUpdateQueue chunkUpdateQueue)
{
    private readonly ChunkUpdateBlockEntry[] buffer = new ChunkUpdateBlockEntry[ChunkVolume];

    public void Receive(ChunkUpdateCommand cmd, ReadOnlySpan<byte> data)
    {
        var blocks = new ChunkBlocks(blocksAllocator);

        BrotliDecoder.TryDecompress(
            data,
            MemoryMarshal.AsBytes(buffer.AsSpan()),
            out var bytes);

        int count = bytes / Marshal.SizeOf<ChunkUpdateBlockEntry>();
        var entries = buffer.AsSpan()[..count];
        int cur = 0;
        int index = 0;

        for (int sz = 0; sz < SectionHeight; sz++)
        {
            while (cur < SectionVolume)
            {
                var entry = entries[index++];
                var block = moduleIndices[entry.Value];

                if (entry.Count != SectionVolume)
                {
                    for (int i = 0; i < entry.Count; i++)
                    {
                        var slice = blocks.Slice(sz);
                        blocks.Slice(sz)[cur++] = block;
                    }
                }
                else
                {
                    blocks.Fill(sz, block);
                    cur += SectionVolume;
                }
            }

            cur = 0;
        }

        chunkUpdateQueue.Enqueue((cmd.Cloc, blocks));
    }
}
