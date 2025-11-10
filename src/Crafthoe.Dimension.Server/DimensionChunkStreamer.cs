namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionChunkStreamer(
    WorldModuleIndices moduleIndices,
    DimensionBlocksRaw blocksRaw)
{
    private readonly ChunkUpdateBlockEntry[] buffer = new ChunkUpdateBlockEntry[ChunkVolume];
    private readonly byte[] data = new byte[ChunkVolume * Marshal.SizeOf<ChunkUpdateBlockEntry>()];
    private int bytes;

    public void Stream(NetSocket ns, Vector2i cloc)
    {
        EncodeIntoBuffer(blocksRaw.Memory(cloc).Span);
        ns.Send(new ChunkUpdateCommand() { Cloc = cloc }, data.AsSpan()[..bytes]);
    }

    private void EncodeIntoBuffer(ReadOnlySpan<Ent> blocks)
    {
        int count = 0;
        Ent prev = default;
        int run = 0;

        foreach (var block in blocks)
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

        BrotliEncoder.TryCompress(
            MemoryMarshal.AsBytes(buffer.AsSpan()[..count]),
            data,
            out var compressedBytes);

        bytes = compressedBytes;

        void Flush()
        {
            if (run > 0)
                buffer[count++] = new() { Value = moduleIndices[prev], Count = run };
        }
    }
}
