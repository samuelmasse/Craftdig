namespace Crafthoe.Dimension;

[Player]
public class PlayerMetrics
{
    private readonly TickMetric renderMetric = new(TimeSpan.FromSeconds(1));
    private readonly TickMetric tickMetric = new(TimeSpan.FromSeconds(1));
    private readonly TickMetricWindow tickMetricWindow;

    public TickMetric RenderMetric => renderMetric;
    public TickMetric TickMetric => tickMetric;
    public TickMetricWindow TickMetricWindow => tickMetricWindow;

    public PlayerMetrics()
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
