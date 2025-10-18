namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionLoader(
    DimensionMetrics metrics,
    DimensionSectionThreads sectionThreads)
{
    public void Run()
    {
        metrics.Start();
        sectionThreads.Start();
    }
}
