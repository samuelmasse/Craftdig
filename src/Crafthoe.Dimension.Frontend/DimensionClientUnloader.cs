namespace Crafthoe.Dimension.Frontend;

[DimensionLoader]
public class DimensionClientUnloader(
    DimensionSectionThreads sectionThreads)
{
    public void Run()
    {
        sectionThreads.Stop();
    }
}
