namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionChunkStreamer(
    WorldModuleIndices moduleIndices,
    DimensionBlocksRaw blocksRaw,
    DimensionLightStreamer lightStreamer,
    DimensionChunkLightFormat lightFormat)
{
    private readonly ChunkUpdateBlockEntry[] buffer = new ChunkUpdateBlockEntry[ChunkVolume];
    private readonly byte[] data = new byte[
        BrotliEncoder.GetMaxCompressedLength(ChunkVolume * Marshal.SizeOf<ChunkUpdateBlockEntry>())];

    public bool Stream(NetSocket ns, Vec2i cloc)
    {
        if (!blocksRaw.TryGetChunkBlocks(cloc, out var blocks))
            return false;

        if (!TryCompress(blocks, out var compressed))
            return false;

        if (!lightStreamer.TryEncode(
            cloc,
            lightFormat.AllSectionsMask,
            true,
            out var lightCommand,
            out var lightData))
            return false;

        ns.Send(new ChunkUpdateCommand() { Cloc = cloc }, compressed);
        ns.Send(lightCommand, lightData);
        return ns.Connected;
    }

    private bool TryCompress(ChunkBlocks blocks, out ReadOnlySpan<byte> compressed)
    {
        int count = 0;

        for (int sz = 0; sz < SectionHeight; sz++)
        {
            var section = blocks.ReadSection(sz, out var uniform);
            if (section.IsEmpty)
            {
                buffer[count++] = new() { Value = moduleIndices[uniform], Count = SectionVolume };
                continue;
            }

            Ent previous = section[0];
            int run = 1;
            for (int i = 1; i < section.Length; i++)
            {
                var block = section[i];
                if (block == previous)
                {
                    run++;
                    continue;
                }

                Flush();
                previous = block;
                run = 1;
            }

            Flush();

            void Flush()
            {
                buffer[count++] = new() { Value = moduleIndices[previous], Count = run };
            }
        }

        if (!BrotliEncoder.TryCompress(
                MemoryMarshal.AsBytes(buffer.AsSpan(0, count)),
                data,
                out int compressedBytes))
        {
            compressed = default;
            return false;
        }

        compressed = data.AsSpan(0, compressedBytes);
        return true;
    }
}
