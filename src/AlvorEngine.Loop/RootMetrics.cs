namespace AlvorEngine.Loop;

[Root]
public class RootMetrics
{
    private readonly TickMetric frameMetric;
    private readonly TickMetricWindow frameMetricWindow;

    internal TickMetric FrameMetric => frameMetric;
    internal TickMetricWindow FrameMetricWindow => frameMetricWindow;

    public TickMetricValue Frame => frameMetric.Value;
    public TickMetricValue FrameWindow => frameMetricWindow.Value;

    public RootMetrics()
    {
        frameMetric = new(TimeSpan.FromSeconds(1));
        frameMetricWindow = new(frameMetric);
    }

    internal void Start()
    {
        frameMetricWindow.Start();
    }

    internal void Stop()
    {
        frameMetricWindow.Stop();
    }
}
