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
        context.AddPost<Vector3d, DimensionComponents.Position>(rigidSorter.Tick);
        context.AddPost<bool, DimensionComponents.IsRigid>(rigidSorter.Tick);
        context.AddPost<bool, WorldComponents.IsLoaded>(rigidSorter.Tick);
    }
}
