namespace Crafthoe.Dimension.Backend;

[Dimension]
public class DimensionChunkCollector(
    DimensionChunkRequester chunkRequester,
    DimensionChunkBag chunkBag,
    DimensionPlayerBag playerBag,
    DimensionChunkUnloader chunkUnloader)
{
    private readonly Stopwatch watch = new();
    private long index;

    public void Frame()
    {
        var iters = Math.Min(chunkBag.Ents.Length, 200);

        watch.Restart();

        while (watch.Elapsed.TotalMilliseconds < 0.25 && iters > 0)
        {
            var chunk = chunkBag.Ents[(int)(index % chunkBag.Ents.Length)];

            if (ShouldCollect(chunk))
                Collect(chunk);

            index++;
            iters--;
        }
    }

    private bool ShouldCollect(Ent chunk)
    {
        var far = chunkRequester.Far;

        foreach (var player in playerBag.Ents)
        {
            var pcloc = player.Position().ToLoc().Xy.ToCloc();

            var delta = Vector2i.Abs(chunk.Cloc() - pcloc);
            var dist = delta.X + delta.Y;
            if (dist < far + 5)
                return false;
        }

        return true;
    }

    private void Collect(Ent chunk) => chunkUnloader.Unload(chunk.Cloc());
}
