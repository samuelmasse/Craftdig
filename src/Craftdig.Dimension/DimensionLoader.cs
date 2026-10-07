namespace Craftdig;

[DimensionLoader]
public class DimensionLoader(
    DimensionEntIdxContext context,
    DimensionEntIndex entIndex,
    DimensionChunkEntIdxContext chunkContext,
    DimensionChunkBag chunkBag,
    DimensionPlayerBag playerBag,
    DimensionRigidBag rigidBag,
    DimensionSeerBag seerBag,
    DimensionChunkRigids chunkRigids)
{
    public void Run()
    {
        context.AddIndex(entIndex.Remove).OnChange<Guid, WorldComponents.Id>(entIndex.Update);
        context.AddBag(seerBag);
        context.AddGatedBag(playerBag);
        context.AddGatedBag(rigidBag);
        context.AddIndex(chunkRigids.Remove)
            .OnChange<Vec3d, DimensionComponents.Position>(chunkRigids.UpdatePosition)
            .OnChange<bool, DimensionComponents.IsRigid>(chunkRigids.UpdateRigid);
        chunkContext.AddGatedBag(chunkBag);
    }
}
