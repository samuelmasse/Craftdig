namespace Craftdig.Server;

[Server]
public class ServerTicks(
    Log log,
    WorldTick tick,
    ServerTickCheck tickCheck,
    ServerTick serverTick)
{
    private Thread? thread;
    private bool stop;

    public void Start()
    {
        thread = new(Run);
        thread.Start();

        log.Trace("Ticks started");
    }

    public void Stop() => stop = true;

    public void Join()
    {
        thread?.Join();
        log.Trace("Ticks stopped");
    }

    public void Run()
    {
        var sw = Stopwatch.StartNew();
        double prev = 0;

        while (!stop)
        {
            tickCheck.Wait();

            var time = sw.Elapsed.TotalSeconds;
            var dt = time - prev;
            prev = time;

            int ticks = tick.Update(dt);
            while (ticks > 0)
            {
                var start = sw.Elapsed.TotalMilliseconds;
                log.Trace("Tick started");

                serverTick.Tick();
                ticks--;

                log.Trace("Tick took {0} ms", Math.Round(sw.Elapsed.TotalMilliseconds - start, 4));
            }
        }
    }
}
