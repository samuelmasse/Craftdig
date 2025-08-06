namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkRequester(
    DimensionPlayerBag playerBag,
    DimensionChunks chunks,
    DimensionChunkLoader chunkLoader)
{
    private readonly int far = 24;
    private readonly Stopwatch watch = new();
    private readonly Random rng = new();

    public int Far => far;

    public void Frame()
    {
        if (playerBag.Ents.IsEmpty)
            return;

        watch.Restart();
        bool next;
        do next = LoadNearestChunk(RandomPlayerChunkLocation());
        while (next && watch.Elapsed.TotalMilliseconds < 1);
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

        float nearest = float.PositiveInfinity;
        bool found = false;

        for (int dy = -far; dy <= far; dy++)
        {
            for (int dx = -far; dx <= far; dx++)
            {
                var ncloc = center + (dx, dy);
                if (chunks.TryGet(ncloc, out _))
                    continue;

                var delta = Vector2i.Abs(center - ncloc);
                var dist = delta.X + delta.Y;
                if (dist > far)
                    continue;

                if (dist >= nearest)
                    continue;

                cloc = ncloc;
                nearest = dist;
                found = true;
            }
        }

        return found;
    }
}
