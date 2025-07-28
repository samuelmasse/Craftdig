namespace AlvorEngine;

public class TickMetric(TimeSpan duration)
{
    private readonly Stopwatch watch = new();
    private readonly Dictionary<DateTime, double> points = [];
    private readonly Queue<DateTime> queue = [];

    private long ticks;
    private double last;
    private double average;
    private double max;

    public TimeSpan Duration => duration;
    public TickMetricValue Value => new(ticks, last, average, max);

    public void Start() => watch.Restart();

    public void End()
    {
        var now = DateTime.UtcNow;

        while (queue.Count > 0 && (now - queue.Peek()).TotalSeconds > duration.TotalSeconds)
            points.Remove(queue.Dequeue());

        double sum = 0;
        max = 0;

        foreach (var point in points)
        {
            if (point.Value > max)
                max = point.Value;

            sum += point.Value;
        }

        average = sum / points.Count;

        watch.Stop();

        last = watch.Elapsed.TotalMilliseconds;
        points.Add(now, last);
        queue.Enqueue(now);

        ticks++;
    }
}
