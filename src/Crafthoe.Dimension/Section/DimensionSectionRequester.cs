namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionRequester(
    DimensionPlayerBag playerBag,
    DimensionChunkRequester chunkRequester,
    DimensionChunks chunks,
    DimensionSections sections,
    DimensionSectionLoader sectionLoader)
{
    private readonly Stopwatch watch = new();
    private readonly Random rng = new();

    public void Frame()
    {
        if (playerBag.Ents.IsEmpty)
            return;

        watch.Restart();
        bool next;
        do next = LoadNearestChunk(RandomPlayerSectionLocation());
        while (next && watch.Elapsed.TotalMilliseconds < 1);
    }

    private Vector3i RandomPlayerSectionLocation()
    {
        var player = playerBag.Ents[rng.Next(playerBag.Ents.Length)];
        return (Vector3i)player.Position() / SectionSize;
    }

    private bool LoadNearestChunk(Vector3i sloc)
    {
        if (!TryGetNeareastChunkWithUnloadedSections(sloc.Xy, out var chunk))
            return false;

        for (int sz = 0; sz < SectionHeight; sz++)
        {
            if (!chunk.Unrendered().ContainsKey(sz))
                continue;

            var nsloc = new Vector3i(chunk.Cloc().X, chunk.Cloc().Y, sz);

            sections.TryGet(nsloc, out var section);
            sectionLoader.Load(section);
        }

        return true;
    }

    private bool TryGetNeareastChunkWithUnloadedSections(Vector2i center, out EntMut val)
    {
        val = default;

        float nearest = float.PositiveInfinity;
        bool found = false;

        for (int dy = -chunkRequester.Far; dy <= chunkRequester.Far; dy++)
        {
            for (int dx = -chunkRequester.Far; dx <= chunkRequester.Far; dx++)
            {
                var ncloc = center + (dx, dy);
                if (!chunks.TryGet(ncloc, out var chunk) || !chunk.IsReadyToRender() || chunk.Unrendered().Count == 0)
                    continue;

                var delta = Vector2i.Abs(center - ncloc);
                var dist = delta.X + delta.Y;
                if (dist > chunkRequester.Far)
                    continue;

                if (dist >= nearest)
                    continue;

                val = chunk;
                nearest = dist;
                found = true;
            }
        }

        return found;
    }
}
