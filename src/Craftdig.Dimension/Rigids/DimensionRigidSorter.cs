namespace Craftdig.Dimension;

[Dimension]
public class DimensionRigidSorter(DimensionChunks chunks, DimensionRigidBag rigidBag)
{
    public void Tick()
    {
        foreach (var ent in rigidBag.Ents)
        {
            var cloc = ent.Position().ToLoc().Xy.ToCloc();
            if (ent.RigidCloc() == cloc)
                continue;

            if (!chunks.TryGet(cloc, out var chunk))
                continue;

            ref var chunkRigids = ref chunk.ChunkRigids();
            chunkRigids ??= [];

            chunkRigids.Add(ent);
            ent.RigidCloc() = cloc;
        }
    }
}
