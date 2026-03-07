namespace Craftdig.Dimension;

[Dimension]
public class DimensionRigidSorter(DimensionChunks chunks, DimensionRigidBag rigidBag)
{
    public void Tick()
    {
        foreach (var rigid in rigidBag.Ents)
            Tick(rigid);
    }

    public void Tick(EntMutIdx ent)
    {
        Vector2i? cloc = (ent.IsLoaded && ent.IsRigid) ? ent.Position.ToLoc().Xy.ToCloc() : null;
        var prevCloc = ent.RigidCloc;

        if (prevCloc == cloc)
            return;

        if (prevCloc != null)
        {
            if (chunks.TryGet(prevCloc.Value, out var prevChunk))
            {
                var chunkRigids = prevChunk.ChunkRigids ??= [];
                chunkRigids.Remove(ent);
            }

            ent.RigidCloc = null;
        }

        if (cloc != null && chunks.TryGet(cloc.Value, out var chunk))
        {
            var chunkRigids = chunk.ChunkRigids ??= [];
            chunkRigids.Add(ent);
            ent.RigidCloc = cloc;
        }
    }
}
