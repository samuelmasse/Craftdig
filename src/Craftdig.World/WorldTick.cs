namespace Craftdig.World;

public class WorldTick
{
    private readonly double rate = 20;
    private double accumulator;

    public double Alpha => accumulator / Interval;
    public double Interval => 1 / rate;

    public int Update(double delta)
    {
        accumulator += delta;
        int ticks = (int)(accumulator * rate);
        accumulator -= ticks / rate;
        return ticks;
    }
}
