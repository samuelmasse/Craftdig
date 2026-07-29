namespace Craftdig.Dimension.Frontend;

[DimensionLoader]
public class DimensionFrontendLoader(
    DimensionEntIdxContextBuilder context,
    DimensionBlockParticleBagMut blockParticles,
    DimensionSectionThreads sectionThreads)
{
    public void Run()
    {
        context.AddBag(blockParticles);
        sectionThreads.Start();
    }
}
