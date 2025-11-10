namespace Crafthoe.Server;

[Server]
public class ServerTicks(WorldTick tick, WorldDimensionBag dimensions, ServerTickCheck tickCheck)
{
    private Thread? thread;
    private bool stop;

    public void Start()
    {
        thread = new(Run);
        thread.Start();

        Console.WriteLine("Ticks started");
    }

    public void Stop() => stop = true;

    public void Join()
    {
        thread?.Join();
        Console.WriteLine("Ticks stopped");
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
                foreach (var dimension in dimensions.Ents)
                    dimension.DimensionScope().Get<DimensionServer>().Tick();

                ticks--;
            }
        }
    }
}
