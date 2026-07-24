namespace Craftdig.Dimension;

[DimensionLoader]
public class DimensionLoader(
    DimensionEntIdxContextBuilder context,
    DimensionEntIndex entIndex,
    DimensionChunkEntIdxContextBuilder chunkContext,
    DimensionChunkBagMut chunkBag,
    DimensionPlayerBagMut playerBag,
    DimensionRigidBagMut rigidBag,
    DimensionSeerBagMut seerBag,
    DimensionChunkRigids chunkRigids)
{
    public void Run()
    {
        context.AddPreDispose(entIndex.InterceptDispose);
        context.AddPre<Guid, WorldComponents.Id>(entIndex.Intercept);
        context.AddBag(seerBag);
        context.AddGatedBag(playerBag);
        context.AddGatedBag(rigidBag);
        context.AddPost<Vec3d, DimensionComponents.Position>(chunkRigids.Intercept);
        context.AddPost<bool, DimensionComponents.IsRigid>(chunkRigids.Intercept);
        chunkContext.AddGatedBag(chunkBag);
    }
}
