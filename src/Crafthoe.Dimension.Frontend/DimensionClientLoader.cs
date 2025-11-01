namespace Crafthoe.Dimension.Frontend;

[DimensionLoader]
public class DimensionClientLoader(
    DimensionSectionThreads sectionThreads)
{
    public void Run()
    {
        sectionThreads.Start();
    }
}
