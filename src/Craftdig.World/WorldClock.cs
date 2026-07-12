namespace Craftdig.World;

[World]
public class WorldClock(WorldUniverse universe)
{
    private readonly long dayTicks = 24000;

    public long Time => universe.Time;
    public long DayTicks => dayTicks;

    public void Tick() => universe.Time++;
}
