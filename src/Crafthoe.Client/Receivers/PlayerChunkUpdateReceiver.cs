namespace Crafthoe.Client;

[Player]
public class PlayerChunkUpdateReceiver(
    WorldModuleIndices moduleIndices,
    DimensionBlocksPool blocksPool,
    PlayerChunkUpdateQueue chunkUpdateQueue)
{
    private readonly ChunkUpdateBlockEntry[] buffer = new ChunkUpdateBlockEntry[ChunkVolume];

    public void Receive(NetSocket ns, NetMessage msg)
    {
        var cloc = MemoryMarshal.Cast<byte, Vector2i>(msg.Data)[0];
        var compressed = msg.Data[Marshal.SizeOf<Vector2i>()..];

        var blocks = blocksPool.Take();

        BrotliDecoder.TryDecompress(
            compressed,
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

        Console.WriteLine($"Chunk {cloc} {msg.Data.Length}");
        chunkUpdateQueue.Enqueue((cloc, blocks));
    }
}
