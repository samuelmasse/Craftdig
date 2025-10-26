namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionLoader(
    DimensionMetrics metrics)
{
    public void Run()
    {
        metrics.Start();
    }
}
