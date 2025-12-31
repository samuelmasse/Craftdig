namespace Craftdig.Dimension.Frontend;

[DimensionLoader]
public class DimensionFrontendLoader(
    DimensionSectionThreads sectionThreads)
{
    public void Run()
    {
        sectionThreads.Start();
    }
}
