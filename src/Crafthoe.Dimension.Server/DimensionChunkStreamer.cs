namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionChunkStreamer(
    WorldModuleIndices moduleIndices,
    DimensionBlocksRaw blocksRaw)
{
    private readonly ChunkUpdateBlockEntry[] buffer = new ChunkUpdateBlockEntry[ChunkVolume];
    private readonly byte[] data = new byte[ChunkVolume * Marshal.SizeOf<ChunkUpdateBlockEntry>()];

    public void Stream(NetSocket ns, Vector2i cloc)
    {
        if (!blocksRaw.TryGetChunkBlocks(cloc, out var blocks))
            return;

        var compressed = Compress(blocks);
        ns.Send(new ChunkUpdateCommand() { Cloc = cloc }, compressed);
    }

    private Span<byte> Compress(ChunkBlocks blocks)
    {
        int count = 0;

        for (int sz = 0; sz < SectionHeight; sz++)
        {
            Ent prev = default;
            int run = 0;

            if (blocks.Uniform(sz) == default)
            {
                foreach (var block in blocks.Slice(sz))
                {
                    if (block != prev)
                    {
                        Flush();
                        prev = block;
                        run = 1;
                    }
                    else run++;
                }

                Flush();

                void Flush()
                {
                    if (run > 0)
                    {
                        buffer[count++] = new() { Value = moduleIndices[prev], Count = run };
                        if (buffer[count - 1].Value == 0)
                        {

                        }
                    }
                }
            }
            else
            {
                var uni = blocks.Uniform(sz);
                buffer[count++] = new() { Value = moduleIndices[uni], Count = SectionVolume };
                if (buffer[count - 1].Value == 0)
                {
                    blocks.Uniform(sz);
                }
            }
        }

        var span = buffer.AsSpan()[..count];
        foreach (var item in span)
        {
            if (item.Value == 0)
            {

            }
        }

        BrotliEncoder.TryCompress(
            MemoryMarshal.AsBytes(buffer.AsSpan()[..count]),
            data,
            out var compressedBytes);

        return data.AsSpan()[..compressedBytes];
    }
}
