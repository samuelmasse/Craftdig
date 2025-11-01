namespace Crafthoe.Dimension.Server;

[Dimension]
public class DimensionChunkStreamer(
    WorldModuleIndices moduleIndices,
    DimensionBlocksRaw blocksRaw)
{
    private readonly ChunkUpdateBlockEntry[] buffer = new ChunkUpdateBlockEntry[ChunkVolume];
    private readonly byte[] data = new byte[ChunkVolume * ChunkUpdateBlockEntry.Size + Marshal.SizeOf<Vector2i>()];
    private int bytes;

    public void Stream(NetSocket ns, Vector2i cloc)
    {
        EncodeIntoBuffer(cloc, blocksRaw.Memory(cloc).Span);
        ns.Send(new((int)ClientCommand.ChunkUpdate, data.AsSpan()[..bytes]));
    }

    private void EncodeIntoBuffer(Vector2i cloc, ReadOnlySpan<Ent> blocks)
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

        MemoryMarshal.Cast<byte, Vector2i>(data.AsSpan())[0] = cloc;
        bytes = Marshal.SizeOf<Vector2i>();

        BrotliEncoder.TryCompress(
            MemoryMarshal.AsBytes(buffer.AsSpan()[..count]),
            data.AsSpan()[bytes..],
            out var compressedBytes);

        bytes += compressedBytes;

        void Flush()
        {
            if (run > 0)
                buffer[count++] = new() { Value = moduleIndices[prev], Count = run };
        }
    }
}
