namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionClientLoader(
    DimensionSectionThreads sectionThreads)
{
    public void Run()
    {
        sectionThreads.Start();
    }
}
