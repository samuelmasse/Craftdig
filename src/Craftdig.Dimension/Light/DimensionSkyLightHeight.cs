namespace Craftdig.Dimension;

[Dimension]
public class DimensionSkyLightHeight
{
    public int Find(ChunkBlocks blocks, int x, int y)
    {
        int column = ((y & SectionMask) << SectionBits) | (x & SectionMask);
        for (int sz = SectionHeight - 1; sz >= 0; sz--)
        {
            var section = blocks.ReadSection(sz, out var uniform);
            if (section.IsEmpty)
            {
                if (uniform.LightOpacity != 0)
                    return (sz + 1) * SectionSize;

                continue;
            }

            for (int z = SectionMask; z >= 0; z--)
            {
                int index = (z << (SectionBits * 2)) | column;
                if (section[index].LightOpacity != 0)
                    return sz * SectionSize + z + 1;
            }
        }

        return 0;
    }
}
