namespace AlvorEngine;

public class TickMetricWindow(TickMetric metric)
{
    private Timer? timer;
    private long prevTicks;
    private TickMetricValue value;

    public TickMetricValue Value => value;

    public void Start() => timer = new((x) => Timer(), null, TimeSpan.Zero, metric.Duration);
    public void Stop() => timer?.Dispose();

    private void Timer()
    {
        long currentTicks = metric.Value.Ticks;
        long delta = currentTicks - prevTicks;
        value = metric.Value with { Ticks = delta };
        prevTicks = currentTicks;
    }
}
