namespace Craftdig.Client;

[Player]
public class PlayerChunkUpdateReceiver(
    WorldModuleIndices moduleIndices,
    DimensionBlocksAllocator blocksAllocator,
    PlayerChunkUpdateQueue chunkUpdateQueue)
{
    private readonly int entrySize = Marshal.SizeOf<ChunkUpdateBlockEntry>();
    private readonly ChunkUpdateBlockEntry[] buffer = new ChunkUpdateBlockEntry[ChunkVolume];

    public void Receive(ChunkUpdateCommand cmd, ReadOnlySpan<byte> data)
    {
        BrotliDecoder.TryDecompress(data, MemoryMarshal.AsBytes(buffer.AsSpan()), out int bytes);

        var entries = buffer.AsSpan(0, bytes / entrySize);

        var blocks = new ChunkBlocks(blocksAllocator);
        int index = 0;

        for (int sz = 0; sz < SectionHeight; sz++)
        {
            var first = entries[index];
            if (first.Count == SectionVolume)
            {
                blocks.Fill(sz, moduleIndices[first.Value]);
                index++;
                continue;
            }

            var destination = blocks.Slice(sz);
            int offset = 0;
            while (offset < SectionVolume)
            {
                var entry = entries[index++];
                destination.Slice(offset, entry.Count).Fill(moduleIndices[entry.Value]);
                offset += entry.Count;
            }
        }

        chunkUpdateQueue.Enqueue((cmd.Cloc, blocks));
    }
}
