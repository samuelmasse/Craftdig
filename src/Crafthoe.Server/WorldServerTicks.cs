namespace Crafthoe.Server;

[World]
public class WorldServerTicks(WorldTick tick, WorldDimensionBag dimensions, WorldServerTickCheck tickCheck)
{
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
