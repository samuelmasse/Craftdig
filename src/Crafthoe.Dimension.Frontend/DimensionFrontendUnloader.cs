namespace Craftdig.Dimension.Frontend;

[DimensionLoader]
public class DimensionFrontendUnloader(
    DimensionSectionThreads sectionThreads)
{
    public void Run()
    {
        sectionThreads.Stop();
    }
}
