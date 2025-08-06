namespace Crafthoe.World;

public class WorldTick
{
    private const double duration = 20;
    private double accumulator;

    public double Alpha => accumulator / (1 / duration);

    public int Update(double delta)
    {
        accumulator += delta;
        int ticks = (int)(accumulator * duration);
        accumulator -= ticks / duration;
        return ticks;
    }
}
