namespace Craftdig.Dimension;

[DimensionLoader]
public class DimensionLoader(
    DimensionEntIdxContextBuilder context,
    DimensionChunkBagMut chunkBag,
    DimensionPlayerBagMut playerBag,
    DimensionRigidBagMut rigidBag,
    DimensionSeerBagMut seerBag,
    DimensionChunkRigids chunkRigids)
{
    public void Run()
    {
        context.AddBagUnloaded(seerBag);
        context.AddBag(chunkBag);
        context.AddBag(playerBag);
        context.AddBag(rigidBag);
        context.AddPost<Vector3d, DimensionComponents.Position>(chunkRigids.Intercept);
        context.AddPost<bool, DimensionComponents.IsRigid>(chunkRigids.Intercept);
    }
}
