namespace Crafthoe.Dimension;

[DimensionLoader]
public class DimensionUnloader(
    DimensionMetrics metrics)
{
    public void Run()
    {
        metrics.Stop();
    }
}
