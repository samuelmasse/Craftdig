namespace Crafthoe.Client;

public class EntDecompressor(WorldModuleIndices moduleIndices, int size)
{
    private readonly ChunkUpdateBlockEntry[] buffer = new ChunkUpdateBlockEntry[ChunkVolume];

    public void Decompress(ReadOnlySpan<byte> data, Span<Ent> output)
    {
        BrotliDecoder.TryDecompress(
            data,
            MemoryMarshal.AsBytes(buffer.AsSpan()),
            out var bytes);

        int count = bytes / Marshal.SizeOf<ChunkUpdateBlockEntry>();
        var entries = buffer.AsSpan()[..count];
        int cur = 0;

        foreach (var entry in entries)
        {
            var block = moduleIndices[entry.Value];

            for (int i = 0; i < entry.Count; i++)
                output[cur++] = block;
        }
    }
} 
