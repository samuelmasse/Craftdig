namespace Craftdig.Dimension;

[Dimension]
public class DimensionRigidSorter(DimensionChunks chunks, DimensionRigidBag rigidBag)
{
    public void Tick()
    {
        foreach (var ent in rigidBag.Ents)
            Tick(ent);
    }

    private void Tick(EntMutIdx ent)
    {
        var cloc = ent.Position.ToLoc().Xy.ToCloc();
        if (ent.RigidCloc == cloc)
            return;

        if (!chunks.TryGet(cloc, out var chunk))
            return;

        var chunkRigids = chunk.ChunkRigids ??= [];
        chunkRigids.Add(ent);
        ent.RigidCloc = cloc;
    }
}
