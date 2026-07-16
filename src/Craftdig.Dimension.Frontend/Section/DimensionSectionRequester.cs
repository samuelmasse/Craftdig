namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionSectionRequester(
    AppRenderDistance renderDistance,
    DimensionSeerBag seerBag,
    DimensionChunks chunks,
    DimensionSections sections,
    DimensionSectionLoader sectionLoader)
{
    private readonly Stopwatch watch = new();
    private readonly Random rng = new();

    public void Frame()
    {
        if (seerBag.Ents.IsEmpty)
            return;

        watch.Restart();
        bool next;
        do next = LoadNearestSection(RandomSeerSectionLocation());
        while (next && watch.Elapsed.TotalMilliseconds < 1);
    }

    private Vec3i RandomSeerSectionLocation()
    {
        var seer = seerBag.Ents[rng.Next(seerBag.Ents.Length)];
        return (Vec3i)seer.Position / SectionSize;
    }

    private bool LoadNearestSection(Vec3i sloc)
    {
        if (!TryGetNearestChunkWithUnloadedSections(sloc.Xy, out var cloc))
            return false;

        chunks.TryGet(cloc, out var chunk);

        int closestIndex = 0;
        int closestDistance = int.MaxValue;

        for (int i = 0; i < chunk.Unrendered.Count; i++)
        {
            int distance = Math.Abs(chunk.Unrendered.Keys[i] - sloc.Z);
            if (distance >= closestDistance)
                continue;

            closestIndex = i;
            closestDistance = distance;
        }

        int sz = chunk.Unrendered.Values[closestIndex];
        var nsloc = new Vec3i(chunk.Cloc.X, chunk.Cloc.Y, sz);

        sections.TryGet(nsloc, out var section);
        sectionLoader.Load(section);

        return true;
    }

    private bool TryGetNearestChunkWithUnloadedSections(Vec2i center, out Vec2i cloc)
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

                bool Visit(Vec2i delta) =>
                    chunks.TryGet(center + delta, out var chunk) && chunk.IsReadyToRender && chunk.Unrendered.Count != 0;
            }
        }

        return false;
    }
}
