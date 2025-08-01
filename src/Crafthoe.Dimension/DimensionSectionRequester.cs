namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionRequester(
    DimensionPlayers players,
    DimensionChunkRequester chunkRequester,
    DimensionChunks chunks,
    DimensionSections sections,
    DimensionSectionLoader sectionLoader)
{
    private readonly Stopwatch watch = new();
    private readonly Random rng = new();

    public void Tick()
    {
        if (players.Players.IsEmpty)
            return;

        watch.Restart();
        bool next;
        do next = LoadNearestSection(RandomPlayerSectionLocation());
        while (next && watch.Elapsed.TotalMilliseconds < 1);
    }

    private Vector3i RandomPlayerSectionLocation()
    {
        var player = players.Players[rng.Next(players.Players.Length)];
        return (Vector3i)player.Position() / SectionSize;
    }

    private bool LoadNearestSection(Vector3i sloc)
    {
        if (!TryGetNeareastChunkWithUnloadedSections(sloc.Xy, out var nearestChunk))
            return false;

        if (!TryGetNearestUnloadedSection(sloc, nearestChunk, out var nearestSection))
            return false;

        sectionLoader.Load(nearestSection);

        return true;
    }

    private bool TryGetNeareastChunkWithUnloadedSections(Vector2i center, out Vector2i cloc)
    {
        cloc = default;

        float nearest = float.PositiveInfinity;
        bool found = false;

        for (int dy = -chunkRequester.Far; dy <= chunkRequester.Far; dy++)
        {
            for (int dx = -chunkRequester.Far; dx <= chunkRequester.Far; dx++)
            {
                var ncloc = center + (dx, dy);
                var unrendered = chunks[ncloc].GetValueOrDefault().ChunkUnrendered();
                if (unrendered == null || unrendered.Count == 0)
                    continue;

                var delta = Vector2i.Abs(center - ncloc);
                var dist = delta.X + delta.Y;
                if (dist > chunkRequester.Far)
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

    private bool TryGetNearestUnloadedSection(Vector3i center, Vector2i cloc, out Vector3i sloc)
    {
        sloc = default;

        var chunk = chunks[cloc].GetValueOrDefault();
        var unrendered = chunk.ChunkUnrendered();
        if (unrendered == null)
            return false;

        float nearest = float.PositiveInfinity;
        bool found = false;

        foreach (var sz in unrendered)
        {
            var nsloc = new Vector3i(cloc.X, cloc.Y, sz);

            if (!sections.TryGet(nsloc, out var section))
                continue;

            float dist = Vector3.DistanceSquared(nsloc, center);
            if (dist >= nearest)
                continue;

            sloc = nsloc;
            nearest = dist;
            found = true;
        }

        return found;
    }
}
