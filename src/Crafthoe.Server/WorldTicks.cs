namespace Crafthoe.Server;

[World]
public class WorldTicks(WorldTick tick, WorldDimensionBag dimensions)
{
    public void Run()
    {
        var sw = Stopwatch.StartNew();
        double prev = 0;

        while (true)
        {
            var time = sw.Elapsed.TotalMilliseconds;
            var dt = time - prev;
            prev = time;

            int ticks = tick.Update(dt);
            while (ticks > 0)
            {
                foreach (var dimension in dimensions.Ents)
                {
                    var ctx = dimension.DimensionScope().Get<DimensionContext>();
                    ctx.Tick();
                    ctx.Frame();
                }

                ticks--;
            }

            Thread.Sleep(20);
        }
    }
}
