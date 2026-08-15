namespace Craftdig;

[Dimension]
public class DimensionBlockParticles(
    DimensionEnt dimension,
    DimensionBlocks blocks,
    DimensionBlockChanges blockChanges,
    DimensionBlockParticleSpawner spawner,
    DimensionBlockParticleSimulation simulation)
{
    private const int MaximumBurstsPerFrame = 8;

    public double Alpha => simulation.Alpha;

    public void Update(double delta) => simulation.Update(delta);

    public void Frame()
    {
        var bursts = 0;
        foreach (var change in blockChanges.Span)
        {
            if (bursts >= MaximumBurstsPerFrame)
                return;

            if (!blocks.TryGet(change.Loc, out var current)
                || current != dimension.Air
                || change.Prev == dimension.Air
                || !change.Prev.IsBlock)
                continue;

            spawner.Spawn(change.Loc, change.Prev);
            bursts++;
        }
    }
}
