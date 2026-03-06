namespace Craftdig.Dimension;

[DimensionLoader]
public class DimensionLoader(
    DimensionEntIdxContextBuilder context,
    DimensionChunkBagMut chunkBag,
    DimensionPlayerBagMut playerBag,
    DimensionRigidBagMut rigidBag)
{
    public void Run()
    {
        context.AddBag(chunkBag);
        context.AddBag(playerBag);
        context.AddBag(rigidBag);
    }
}
