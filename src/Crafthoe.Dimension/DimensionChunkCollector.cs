namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunkCollector(
    DimensionChunkRequester chunkRequester,
    DimensionChunkBag chunkIndex,
    DimensionPlayers players,
    DimensionChunkUnloader chunkUnloader)
{
    private long index;

    public void Collect()
    {
        var iters = Math.Min(chunkIndex.Ents.Length, 200);

        for (int i = 0; i < iters; i++)
        {
            var chunk = chunkIndex.Ents[(int)(index % chunkIndex.Ents.Length)];

            if (ShouldCollect(chunk))
                Collect(chunk);

            index++;
        }
    }

    private bool ShouldCollect(Ent chunk)
    {
        var far = chunkRequester.Far;

        foreach (var player in players.Players)
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
