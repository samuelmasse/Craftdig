namespace Crafthoe.Dimension;

[Dimension]
public class DimensionMetrics
{
    private readonly TickMetric chunkMetric = new(TimeSpan.FromSeconds(1));
    private readonly TickMetric sectionMetric = new(TimeSpan.FromSeconds(1));
    private readonly TickMetric renderMetric = new(TimeSpan.FromSeconds(1));

    public TickMetric ChunkMetric => chunkMetric;
    public TickMetric SectionMetric => sectionMetric;
    public TickMetric RenderMetric => renderMetric;
}
