namespace Crafthoe.Dimension;

[Dimension]
public class DimensionMetrics
{
    private readonly TickMetric chunkMetric = new(TimeSpan.FromSeconds(1));
    private readonly TickMetric sectionMetric = new(TimeSpan.FromSeconds(1));
    private readonly TickMetric renderMetric = new(TimeSpan.FromSeconds(1));
    private readonly TickMetric tickMetric = new(TimeSpan.FromSeconds(1));
    private readonly TickMetricWindow tickMetricWindow;

    public TickMetric ChunkMetric => chunkMetric;
    public TickMetric SectionMetric => sectionMetric;
    public TickMetric RenderMetric => renderMetric;
    public TickMetric TickMetric => tickMetric;
    public TickMetricWindow TickMetricWindow => tickMetricWindow;

    public DimensionMetrics()
    {
        tickMetricWindow = new(tickMetric);
    }

    public void Start()
    {
        tickMetricWindow.Start();
    }

    public void Stop()
    {
        tickMetricWindow.Stop();
    }
}
