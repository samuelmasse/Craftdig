namespace Crafthoe.Dimension.Server;

public class EntCompressor(WorldModuleIndices moduleIndices, int size)
{
    private readonly ChunkUpdateBlockEntry[] buffer = new ChunkUpdateBlockEntry[size];
    private readonly byte[] data = new byte[size * Marshal.SizeOf<ChunkUpdateBlockEntry>()];

    public Span<byte> Compress(ReadOnlySpan<Ent> blocks)
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

        return data.AsSpan()[..compressedBytes];

        void Flush()
        {
            if (run > 0)
                buffer[count++] = new() { Value = moduleIndices[prev], Count = run };
        }
    }
}
