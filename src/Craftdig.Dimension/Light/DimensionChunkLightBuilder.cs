namespace Craftdig;

[Dimension]
public class DimensionChunkLightBuilder(
    DimensionEnt dimension,
    DimensionLightAllocator allocator,
    DimensionLightPropagation propagation,
    DimensionSkyLightHeight skyLightHeight)
{
    private readonly ConcurrentBag<WorkState> states = [];
    private readonly bool hasSkyLight = dimension.HasSkyLight;

    public ChunkLight Build(ChunkBlocks blocks, Vec2i cloc)
    {
        if (!states.TryTake(out var state))
            state = new();

        var light = new ChunkLight(allocator);
        BuildSkyHeight(blocks, light);
        SeedBlockSources(blocks, cloc, light, state.BlockIncrease);
        RunIncrease(blocks, cloc, light, LightChannel.Block, state.BlockIncrease);

        if (hasSkyLight)
        {
            SeedSkySources(blocks, cloc, light, state.SkyIncrease);
            RunIncrease(blocks, cloc, light, LightChannel.Sky, state.SkyIncrease);
        }

        states.Add(state);
        return light;
    }

    private void BuildSkyHeight(ChunkBlocks blocks, ChunkLight light)
    {
        for (int y = 0; y < SectionSize; y++)
        {
            for (int x = 0; x < SectionSize; x++)
                light.SkyHeight[(y << SectionBits) | x] = (ushort)skyLightHeight.Find(blocks, x, y);
        }
    }

    private void SeedBlockSources(
        ChunkBlocks blocks,
        Vec2i cloc,
        ChunkLight light,
        Queue<LightNode> queue)
    {
        var origin = cloc * SectionSize;
        for (int sz = 0; sz < SectionHeight; sz++)
        {
            var section = blocks.ReadSection(sz, out var uniform);
            if (section.IsEmpty)
            {
                if (uniform.LightEmission == 0)
                    continue;

                for (int index = 0; index < SectionVolume; index++)
                    Seed(index, uniform.LightEmission);
                continue;
            }

            for (int index = 0; index < section.Length; index++)
            {
                byte emission = section[index].LightEmission;
                if (emission != 0)
                    Seed(index, emission);
            }

            void Seed(int index, byte emission)
            {
                var loc = new Vec3i(
                    origin.X + (index & SectionMask),
                    origin.Y + ((index >> SectionBits) & SectionMask),
                    sz * SectionSize + (index >> (SectionBits * 2)));
                light.SetStored(LightChannel.Block, loc, emission);
                queue.Enqueue(new(loc, emission));
            }
        }
    }

    private void SeedSkySources(
        ChunkBlocks blocks,
        Vec2i cloc,
        ChunkLight light,
        Queue<LightNode> queue)
    {
        var origin = cloc * SectionSize;
        for (int y = 0; y < SectionSize; y++)
        {
            for (int x = 0; x < SectionSize; x++)
            {
                int wx = origin.X + x;
                int wy = origin.Y + y;
                int height = light.SkyHeight[(y << SectionBits) | x];

                if (height < HeightSize)
                {
                    int end = Math.Max(height + 1, NeighborHeight(x - 1, y));
                    end = Math.Max(end, NeighborHeight(x + 1, y));
                    end = Math.Max(end, NeighborHeight(x, y - 1));
                    end = Math.Max(end, NeighborHeight(x, y + 1));

                    // Direct skylight is implicit in ChunkLight. Queue every direct-sky
                    // cell beside a taller column so it can spread sideways beneath
                    // overhangs; queuing only the lowest cell misses those openings.
                    for (int z = height; z < end; z++)
                        queue.Enqueue(new((wx, wy, z), LightLevel.Max));
                }
                else
                {
                    var topLoc = new Vec3i(wx, wy, HeightSize - 1);
                    var top = blocks[topLoc];
                    byte value = top.LightOpacity >= LightLevel.Max
                        ? (byte)0
                        : (byte)(LightLevel.Max - top.LightOpacity);
                    if (value != 0)
                    {
                        light.SetStored(LightChannel.Sky, topLoc, value);
                        queue.Enqueue(new(topLoc, value));
                    }
                }

                int NeighborHeight(int nx, int ny) =>
                    (uint)nx < SectionSize && (uint)ny < SectionSize
                        ? light.SkyHeight[(ny << SectionBits) | nx]
                        : height;
            }
        }
    }

    private void RunIncrease(
        ChunkBlocks blocks,
        Vec2i cloc,
        ChunkLight light,
        LightChannel channel,
        Queue<LightNode> queue)
    {
        int minX = cloc.X * SectionSize;
        int minY = cloc.Y * SectionSize;
        int maxX = minX + SectionMask;
        int maxY = minY + SectionMask;

        while (queue.TryDequeue(out var node))
        {
            byte current = channel == LightChannel.Sky ? light.Sky(node.Loc) : light.Block(node.Loc);
            byte level = Math.Max(current, node.Level);
            if (level == 0)
                continue;

            if (level > current && (channel != LightChannel.Sky || !light.IsDirectSky(node.Loc)))
                light.SetStored(channel, node.Loc, level);

            foreach (var direction in propagation.Directions)
            {
                var neighbor = node.Loc + direction;
                if ((uint)neighbor.Z >= HeightSize ||
                    neighbor.X < minX || neighbor.X > maxX ||
                    neighbor.Y < minY || neighbor.Y > maxY)
                    continue;

                byte neighborLevel = channel == LightChannel.Sky ? light.Sky(neighbor) : light.Block(neighbor);
                byte contribution = propagation.Contribution(
                    channel,
                    level,
                    blocks[neighbor].LightOpacity,
                    node.Loc.Z,
                    neighbor.Z);
                if (contribution <= neighborLevel)
                    continue;

                if (channel != LightChannel.Sky || !light.IsDirectSky(neighbor))
                    light.SetStored(channel, neighbor, contribution);

                queue.Enqueue(new(neighbor, contribution));
            }
        }
    }

    private sealed class WorkState
    {
        public readonly Queue<LightNode> SkyIncrease = new(SectionVolume);
        public readonly Queue<LightNode> BlockIncrease = new(SectionVolume);
    }
}
