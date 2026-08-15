namespace Craftdig;

[Dimension]
public class DimensionLighting(
    DimensionEnt dimension,
    DimensionBlocksRaw blocks,
    DimensionBlockChanges blockChanges,
    DimensionLightsRaw lights,
    DimensionLightChanges lightChanges,
    DimensionLightPropagation propagation,
    DimensionSkyLightHeight skyLightHeight)
{
    private readonly Queue<LightNode> skyIncrease = new(SectionVolume);
    private readonly Queue<LightNode> skyDecrease = new(SectionVolume);
    private readonly Queue<LightNode> blockIncrease = new(SectionVolume);
    private readonly Queue<LightNode> blockDecrease = new(SectionVolume);

    public void ConnectChunk(Vec2i cloc)
    {
        SeedLoadedBorders(cloc);
        RunIncrease(LightChannel.Block, cloc);
        RunIncrease(LightChannel.Sky, cloc);
    }

    public void Tick()
    {
        foreach (var change in blockChanges.Span)
            ProcessBlockChange(change.Loc, change.Prev);

        RunDecrease(LightChannel.Block);
        RunDecrease(LightChannel.Sky);
        RunIncrease(LightChannel.Block);
        RunIncrease(LightChannel.Sky);
    }

    private void ProcessBlockChange(Vec3i loc, Ent previous)
    {
        if (!blocks.TryGet(loc, out var current) ||
            !lights.TryGetChunkLight(loc.Xy.ToCloc(), out var chunkLight))
            return;

        byte oldBlock = chunkLight.Block(loc);
        byte oldSky = chunkLight.Sky(loc);

        if (previous.LightEmission != current.LightEmission || previous.LightOpacity != current.LightOpacity)
            BeginDecrease(LightChannel.Block, loc, oldBlock);

        if (!dimension.HasSkyLight || previous.LightOpacity == current.LightOpacity)
            return;

        int column = ((loc.Y & SectionMask) << SectionBits) | (loc.X & SectionMask);
        int oldHeight = chunkLight.SkyHeight[column];
        int newHeight = FindSkyHeight(loc.X, loc.Y);
        chunkLight.SkyHeight[column] = (ushort)newHeight;

        if (newHeight > oldHeight)
        {
            for (int z = oldHeight; z < newHeight; z++)
            {
                var changed = new Vec3i(loc.X, loc.Y, z);
                lightChanges.Add(changed);
                BeginDecrease(LightChannel.Sky, changed, LightLevel.Max);
            }
        }
        else if (newHeight < oldHeight)
        {
            for (int z = newHeight; z < oldHeight; z++)
            {
                var changed = new Vec3i(loc.X, loc.Y, z);
                lightChanges.Add(changed);
                QueueIncrease(LightChannel.Sky, changed, LightLevel.Max);
            }
        }
        else BeginDecrease(LightChannel.Sky, loc, oldSky);

        QueueNeighbors(LightChannel.Sky, loc);
    }

    private int FindSkyHeight(int x, int y)
    {
        var cloc = new Vec2i(x, y).ToCloc();
        if (!blocks.TryGetChunkBlocks(cloc, out var chunkBlocks))
            return 0;

        return skyLightHeight.Find(chunkBlocks, x, y);
    }

    private int NeighborSkyHeight(int x, int y, int fallback)
    {
        if (!lights.TryGetChunkLight(new Vec2i(x, y).ToCloc(), out var light))
            return fallback;

        int column = ((y & SectionMask) << SectionBits) | (x & SectionMask);
        return light.SkyHeight[column];
    }

    private void SeedLoadedBorders(Vec2i cloc)
    {
        int minX = cloc.X * SectionSize;
        int minY = cloc.Y * SectionSize;
        int maxX = minX + SectionMask;
        int maxY = minY + SectionMask;

        SeedStoredBoundary((minX, minY), (0, 1));
        SeedStoredBoundary((minX - 1, minY), (0, 1));
        SeedStoredBoundary((maxX, minY), (0, 1));
        SeedStoredBoundary((maxX + 1, minY), (0, 1));
        SeedStoredBoundary((minX, minY), (1, 0));
        SeedStoredBoundary((minX, minY - 1), (1, 0));
        SeedStoredBoundary((minX, maxY), (1, 0));
        SeedStoredBoundary((minX, maxY + 1), (1, 0));

        if (!dimension.HasSkyLight)
            return;

        for (int i = 0; i < SectionSize; i++)
        {
            SeedSkyBoundary((minX, minY + i), (minX - 1, minY + i));
            SeedSkyBoundary((maxX, minY + i), (maxX + 1, minY + i));
            SeedSkyBoundary((minX + i, minY), (minX + i, minY - 1));
            SeedSkyBoundary((minX + i, maxY), (minX + i, maxY + 1));
        }
    }

    private void SeedStoredBoundary(Vec2i first, Vec2i step)
    {
        if (!lights.TryGetChunkLight(first.ToCloc(), out var light))
            return;

        Seed(LightChannel.Block);
        Seed(LightChannel.Sky);

        void Seed(LightChannel channel)
        {
            for (int sz = 0; sz < SectionHeight; sz++)
            {
                if (!light.MayHaveStoredLight(channel, sz))
                    continue;

                for (int z = 0; z < SectionSize; z++)
                {
                    for (int i = 0; i < SectionSize; i++)
                    {
                        var xy = first + step * i;
                        var loc = new Vec3i(xy, sz * SectionSize + z);
                        QueueIncrease(channel, loc, light.Stored(channel, loc));
                    }
                }
            }
        }
    }

    private void SeedSkyBoundary(Vec2i inside, Vec2i outside)
    {
        int insideHeight = NeighborSkyHeight(inside.X, inside.Y, HeightSize);
        int outsideHeight = NeighborSkyHeight(outside.X, outside.Y, HeightSize);
        if (insideHeight == HeightSize || outsideHeight == HeightSize)
            return;

        Vec2i source = insideHeight <= outsideHeight ? inside : outside;
        int start = Math.Min(insideHeight, outsideHeight);
        int end = Math.Max(insideHeight, outsideHeight);
        for (int z = start; z < end; z++)
            QueueIncrease(LightChannel.Sky, new(source, z), LightLevel.Max);
    }

    private void BeginDecrease(LightChannel channel, Vec3i loc, byte oldLevel)
    {
        if (oldLevel == 0)
        {
            SeedBaseline(channel, loc);
            QueueNeighbors(channel, loc);
            return;
        }

        ForceClear(channel, loc);
        DecreaseQueue(channel).Enqueue(new(loc, oldLevel));
    }

    private void RunDecrease(LightChannel channel)
    {
        var queue = DecreaseQueue(channel);
        while (queue.TryDequeue(out var node))
        {
            SeedBaseline(channel, node.Loc);

            foreach (var direction in propagation.Directions)
            {
                var neighbor = node.Loc + direction;
                if (!TryGetLevel(channel, neighbor, out byte neighborLevel) || neighborLevel == 0)
                    continue;

                byte contribution = Contribution(channel, node.Level, node.Loc, neighbor);
                if (contribution != 0 && neighborLevel <= contribution)
                {
                    if (ForceClear(channel, neighbor))
                        queue.Enqueue(new(neighbor, neighborLevel));
                    else QueueIncrease(channel, neighbor, neighborLevel);
                }
                else QueueIncrease(channel, neighbor, neighborLevel);
            }
        }
    }

    private void RunIncrease(LightChannel channel) => RunIncrease(channel, null);

    private void RunIncrease(LightChannel channel, Vec2i? untrackedCloc)
    {
        var queue = IncreaseQueue(channel);
        while (queue.TryDequeue(out var node))
        {
            if (!TryGetLevel(channel, node.Loc, out byte current))
                continue;

            byte level = Math.Max(current, node.Level);
            if (level == 0)
                continue;

            if (level > current)
                SetLevel(channel, node.Loc, level, untrackedCloc);

            foreach (var direction in propagation.Directions)
            {
                var neighbor = node.Loc + direction;
                if (!TryGetLevel(channel, neighbor, out byte neighborLevel))
                    continue;

                byte contribution = Contribution(channel, level, node.Loc, neighbor);
                if (contribution <= neighborLevel)
                    continue;

                if (SetLevel(channel, neighbor, contribution, untrackedCloc))
                    queue.Enqueue(new(neighbor, contribution));
            }
        }
    }

    private byte Contribution(LightChannel channel, byte source, Vec3i from, Vec3i to)
    {
        if (source == 0 || !blocks.TryGet(to, out var block))
            return 0;

        return propagation.Contribution(channel, source, block.LightOpacity, from.Z, to.Z);
    }

    private void QueueNeighbors(LightChannel channel, Vec3i loc)
    {
        foreach (var direction in propagation.Directions)
        {
            var neighbor = loc + direction;
            if (TryGetLevel(channel, neighbor, out byte level))
                QueueIncrease(channel, neighbor, level);
        }

        SeedBaseline(channel, loc);
    }

    private void SeedBaseline(LightChannel channel, Vec3i loc)
    {
        if (channel == LightChannel.Sky)
        {
            if (lights.TryGetChunkLight(loc.Xy.ToCloc(), out var light) && light.IsDirectSky(loc))
                QueueIncrease(channel, loc, LightLevel.Max);
            return;
        }

        if (blocks.TryGet(loc, out var block) && block.LightEmission != 0)
        {
            SetLevel(channel, loc, block.LightEmission);
            QueueIncrease(channel, loc, block.LightEmission);
        }
    }

    private void QueueIncrease(LightChannel channel, Vec3i loc, byte level)
    {
        if (level != 0)
            IncreaseQueue(channel).Enqueue(new(loc, level));
    }

    private bool TryGetLevel(LightChannel channel, Vec3i loc, out byte value)
    {
        if ((uint)loc.Z >= HeightSize || !lights.TryGetChunkLight(loc.Xy.ToCloc(), out var light))
        {
            value = 0;
            return false;
        }

        value = channel == LightChannel.Sky ? light.Sky(loc) : light.Block(loc);
        return true;
    }

    private bool SetLevel(LightChannel channel, Vec3i loc, byte value) =>
        SetLevel(channel, loc, value, null);

    private bool SetLevel(LightChannel channel, Vec3i loc, byte value, Vec2i? untrackedCloc)
    {
        if ((uint)loc.Z >= HeightSize || !lights.TryGetChunkLight(loc.Xy.ToCloc(), out var light))
            return false;

        if (channel == LightChannel.Sky && light.IsDirectSky(loc))
            return false;

        if (!light.SetStored(channel, loc, value))
            return false;

        if (loc.Xy.ToCloc() != untrackedCloc)
            lightChanges.Add(loc);

        return true;
    }

    private bool ForceClear(LightChannel channel, Vec3i loc)
    {
        if ((uint)loc.Z >= HeightSize || !lights.TryGetChunkLight(loc.Xy.ToCloc(), out var light))
            return false;

        if (channel == LightChannel.Sky && light.IsDirectSky(loc))
            return false;

        if (light.SetStored(channel, loc, 0))
        {
            lightChanges.Add(loc);
            return true;
        }

        return false;
    }

    private Queue<LightNode> IncreaseQueue(LightChannel channel) =>
        channel == LightChannel.Sky ? skyIncrease : blockIncrease;

    private Queue<LightNode> DecreaseQueue(LightChannel channel) =>
        channel == LightChannel.Sky ? skyDecrease : blockDecrease;

}
