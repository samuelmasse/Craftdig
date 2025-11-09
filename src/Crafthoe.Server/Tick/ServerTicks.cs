namespace Crafthoe.Server;

[Server]
public class ServerTicks(WorldTick tick, WorldDimensionBag dimensions, ServerTickCheck tickCheck)
{
    private Thread? thread;

    public void Start()
    {
        thread = new(Run);
        thread.Start();
    }

    public void Join() => thread?.Join();

    public void Run()
    {
        var sw = Stopwatch.StartNew();
        double prev = 0;

        while (true)
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
