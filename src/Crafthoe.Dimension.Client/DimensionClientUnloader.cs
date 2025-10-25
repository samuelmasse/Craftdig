namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionClientUnloader(
    DimensionSectionThreads sectionThreads)
{
    public void Run()
    {
        sectionThreads.Stop();
    }
}
