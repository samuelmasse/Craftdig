namespace Craftdig;

[Player]
public class PlayerChunkLightUpdateReceiver(
    DimensionLightAllocator allocator,
    DimensionChunkLightFormat format,
    PlayerChunkLightUpdateQueue queue)
{
    private readonly byte[] uncompressed = new byte[format.MaximumBytes];

    public void Receive(ChunkLightUpdateCommand command, ReadOnlySpan<byte> data)
    {
        bool full = command.Full != 0;
        BrotliDecoder.TryDecompress(data, uncompressed, out _);

        var skyHeight = MemoryMarshal.Cast<byte, ushort>(
            uncompressed.AsSpan(0, format.HeightBytes));

        var light = new ChunkLight(allocator);
        skyHeight.CopyTo(light.SkyHeight);

        int offset = format.HeightBytes;
        for (int sz = 0; sz < SectionHeight; sz++)
        {
            if ((command.SectionMask & (1u << sz)) == 0)
                continue;

            light.LoadSection(
                LightChannel.Sky,
                sz,
                uncompressed.AsSpan(offset, allocator.NibbleVolume));
            offset += allocator.NibbleVolume;

            light.LoadSection(
                LightChannel.Block,
                sz,
                uncompressed.AsSpan(offset, allocator.NibbleVolume));
            offset += allocator.NibbleVolume;
        }

        queue.Enqueue(new(command.Cloc, light, command.SectionMask, full));
    }
}
