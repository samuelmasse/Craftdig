namespace Craftdig.Dimension;

[Dimension]
public class DimensionRigidSorter(DimensionChunks chunks)
{
    public void SortPosition(EntMutIdx ent, Vector3d position) =>
        Sort(ent, ent.IsRigid, position);

    public void SortIsRigid(EntMutIdx ent, bool isRigid) =>
        Sort(ent, isRigid, ent.Position);

    private void Sort(EntMutIdx ent, bool isRigid, Vector3d position)
    {
        Vector2i? cloc = isRigid ? position.ToLoc().Xy.ToCloc() : null;
        var prevCloc = ent.RigidCloc;

        if (prevCloc == cloc)
            return;

        if (prevCloc != null)
        {
            if (!chunks.TryGet(prevCloc.Value, out var prevChunk))
                return;

            var chunkRigids = prevChunk.ChunkRigids ??= [];
            chunkRigids.Remove(ent);
        }

        if (cloc != null)
        {
            if (!chunks.TryGet(cloc.Value, out var chunk))
                return;

            var chunkRigids = chunk.ChunkRigids ??= [];
            chunkRigids.Add(ent);
        }

        ent.RigidCloc = cloc;
    }
}
