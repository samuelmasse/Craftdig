namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionLightStreamer(
    DimensionLightsRaw lights,
    DimensionLightAllocator allocator,
    DimensionChunkLightFormat format)
{
    private readonly byte[] uncompressed = new byte[format.MaximumBytes];
    private readonly byte[] compressed = new byte[BrotliEncoder.GetMaxCompressedLength(format.MaximumBytes)];

    public bool TryEncode(
        Vec2i cloc,
        uint sectionMask,
        bool full,
        out ChunkLightUpdateCommand command,
        out ReadOnlySpan<byte> data)
    {
        command = new()
        {
            Cloc = cloc,
            SectionMask = sectionMask,
            Full = full ? (byte)1 : (byte)0
        };

        if (!lights.TryGetChunkLight(cloc, out var light))
        {
            data = default;
            return false;
        }

        int offset = format.HeightBytes;
        MemoryMarshal.AsBytes(light.SkyHeightReadOnly).CopyTo(uncompressed);

        for (int sz = 0; sz < SectionHeight; sz++)
        {
            if ((sectionMask & (1u << sz)) == 0)
                continue;

            light.CopySection(
                LightChannel.Sky,
                sz,
                uncompressed.AsSpan(offset, allocator.NibbleVolume));
            offset += allocator.NibbleVolume;

            light.CopySection(
                LightChannel.Block,
                sz,
                uncompressed.AsSpan(offset, allocator.NibbleVolume));
            offset += allocator.NibbleVolume;
        }

        if (!BrotliEncoder.TryCompress(uncompressed.AsSpan(0, offset), compressed, out int bytes))
        {
            data = default;
            return false;
        }

        data = compressed.AsSpan(0, bytes);
        return true;
    }
}
