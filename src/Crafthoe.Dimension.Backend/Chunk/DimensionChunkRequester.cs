namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionChunkRequester(
    DimensionDrawDistance drawDistance,
    DimensionPlayerBag playerBag,
    DimensionChunks chunks,
    DimensionChunkThreadWorkQueue chunkThreadWorkQueue,
    DimensionChunkPending chunkPending,
    DimensionChunkLoader chunkLoader)
{
    private readonly Stopwatch watch = new();
    private readonly Random rng = new();

    public void Frame()
    {
        if (playerBag.Ents.IsEmpty)
            return;

        int credits = 32 - chunkThreadWorkQueue.Count;
        bool next = true;

        watch.Restart();

        while (next && credits > 0 && watch.Elapsed.TotalMilliseconds < 1)
        {
            next = LoadNearestChunk(RandomPlayerChunkLocation());
            credits--;
        }
    }

    private Vector2i RandomPlayerChunkLocation()
    {
        var player = playerBag.Ents[rng.Next(playerBag.Ents.Length)];
        return player.Position().ToLoc().Xy.ToCloc();
    }

    private bool LoadNearestChunk(Vector2i cloc)
    {
        if (!TryGetNearestUnloadedChunk(cloc, out var nearest))
            return false;

        chunkLoader.Load(nearest);

        return true;
    }

    private bool TryGetNearestUnloadedChunk(Vector2i center, out Vector2i cloc)
    {
        cloc = default;

        for (int r = 0; r <= drawDistance.Far; r++)
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

                bool Visit(Vector2i delta)
                {
                    return !chunks.TryGet(center + delta, out _) && !chunkPending.Contains(center + delta);
                }
            }
        }

        return false;
    }
}
