namespace Craftdig;

[World]
public class WorldSky(WorldClock clock)
{
    public float Strength
    {
        get
        {
            double phase = clock.Time % clock.DayTicks / (double)clock.DayTicks;
            double sun = Math.Cos((phase - 0.25) * Math.PI * 2);
            return (float)Math.Clamp((sun + 0.2) / 1.2, 0.05, 1);
        }
    }
}
