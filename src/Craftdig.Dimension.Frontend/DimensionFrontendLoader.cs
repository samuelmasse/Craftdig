namespace Craftdig;

[DimensionLoader]
public class DimensionFrontendLoader(
    DimensionEntIdxContext context,
    DimensionBlockParticleBag blockParticles,
    DimensionSectionThreads sectionThreads)
{
    public void Run()
    {
        context.AddBag(blockParticles);
        sectionThreads.Start();
    }
}
