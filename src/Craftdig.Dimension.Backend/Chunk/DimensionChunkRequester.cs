namespace Craftdig;

[Dimension]
public class DimensionChunkRequester(
    AppRenderDistance renderDistance,
    DimensionSeerBag seerBag,
    DimensionChunks chunks,
    DimensionChunkPending chunkPending,
    DimensionChunkLoader chunkLoader)
{
    private readonly Stopwatch watch = new();
    private readonly Random rng = new();

    public void Frame()
    {
        if (seerBag.Ents.IsEmpty)
            return;

        int credits = 32 - chunkPending.Count;
        bool next = true;

        watch.Restart();

        while (next && credits > 0 && watch.Elapsed.TotalMilliseconds < 1)
        {
            next = LoadNearestChunk(RandomSeerChunkLocation());
            credits--;
        }
    }

    private Vec2i RandomSeerChunkLocation()
    {
        var seer = seerBag.Ents[rng.Next(seerBag.Ents.Length)];
        return seer.Position.ToLoc().Xy.ToCloc();
    }

    private bool LoadNearestChunk(Vec2i cloc)
    {
        if (!TryGetNearestUnloadedChunk(cloc, out var nearest))
            return false;

        chunkLoader.Load(nearest);

        return true;
    }

    private bool TryGetNearestUnloadedChunk(Vec2i center, out Vec2i cloc)
    {
        cloc = default;

        for (int r = 0; r <= renderDistance.Far; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int dy = r - Math.Abs(dx);

                if (Visit((dx, dy)))
                {
                    cloc = center + (dx, dy);
                    return true;
                }

                if (Visit((dx, -dy)))
                {
                    cloc = center + (dx, -dy);
                    return true;
                }

                bool Visit(Vec2i delta)
                {
                    return !chunks.TryGet(center + delta, out _) && !chunkPending.Contains(center + delta);
                }
            }
        }

        return false;
    }
}
