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
        if (!TryGetNeareastChunkWithUnloadedSections(sloc.Xy, out var cloc))
            return false;

        var chunk = chunks.Get(cloc);

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

    private bool TryGetNeareastChunkWithUnloadedSections(Vector2i center, out Vector2i cloc)
    {
        cloc = default;

        for (int r = 0; r <= chunkRequester.Far; r++)
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

                bool Visit(Vector2i delta) =>
                    chunks.TryGet(center + delta, out var chunk) && chunk.IsReadyToRender() && chunk.Unrendered().Count != 0;
            }
        }

        return false;
    }
}
