namespace Craftdig.Dimension;

[DimensionLoader]
public class DimensionLoader(
    DimensionEntIdxContextBuilder context,
    DimensionChunkBagMut chunkBag,
    DimensionPlayerBagMut playerBag,
    DimensionRigidBagMut rigidBag,
    DimensionRigidSorter rigidSorter)
{
    public void Run()
    {
        context.AddBag(chunkBag);
        context.AddBag(playerBag);
        context.AddBag(rigidBag);
        context.AddInterceptor<Vector3d, DimensionComponents.Position>(rigidSorter.SortPosition);
        context.AddInterceptor<bool, DimensionComponents.IsRigid>(rigidSorter.SortIsRigid);
    }
}
