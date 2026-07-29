namespace Craftdig.Dimension.Frontend;

[Dimension]
public class DimensionBlockParticleSimulation(
    DimensionBlockParticleBag particles,
    DimensionBlocks blocks)
{
    private const double TickInterval = 1.0 / 20.0;

    private readonly List<EntPtrIdx> expired = [];
    private double accumulator;

    public double Alpha => accumulator / TickInterval;

    public void Update(double delta)
    {
        accumulator += Math.Min(delta, 0.1);
        while (accumulator >= TickInterval)
        {
            Tick();
            accumulator -= TickInterval;
        }
    }

    private void Tick()
    {
        foreach (var particle in particles.Ents)
            Tick(particle);

        foreach (var allocation in expired)
            allocation.Dispose();

        expired.Clear();
    }

    private void Tick(EntMutIdx particle)
    {
        particle.BlockParticlePrevPosition =
            particle.BlockParticlePosition;
        particle.BlockParticleAge++;
        if (particle.BlockParticleAge >= particle.BlockParticleMaxAge)
        {
            expired.Add(particle.BlockParticleAllocation);
            return;
        }

        var velocity = particle.BlockParticleVelocity;
        velocity.Z -= 0.04;
        var position = particle.BlockParticlePosition;
        var radius = Math.Max(
            0.025,
            particle.BlockParticleSize * 0.28);

        if (!TryMove(ref position, 0, velocity.X, radius))
            velocity.X = 0;
        if (!TryMove(ref position, 1, velocity.Y, radius))
            velocity.Y = 0;
        var movedZ = TryMove(ref position, 2, velocity.Z, radius);
        var onGround = !movedZ && velocity.Z < 0;
        if (!movedZ)
            velocity.Z = 0;

        velocity *= 0.98;
        if (onGround)
        {
            velocity.X *= 0.7;
            velocity.Y *= 0.7;
        }

        particle.BlockParticlePosition = position;
        particle.BlockParticleVelocity = velocity;
    }

    private bool TryMove(
        ref Vec3d position,
        int axis,
        double amount,
        double radius)
    {
        if (Math.Abs(amount) < 0.000001)
            return true;

        var next = axis switch
        {
            0 => position + new Vec3d(amount, 0, 0),
            1 => position + new Vec3d(0, amount, 0),
            _ => position + new Vec3d(0, 0, amount)
        };
        if (Collides(next, radius))
            return false;

        position = next;
        return true;
    }

    private bool Collides(Vec3d center, double radius)
    {
        var min = new Vec3d(
            center.X - radius,
            center.Y - radius,
            center.Z - radius).ToLoc();
        var max = new Vec3d(
            center.X + radius,
            center.Y + radius,
            center.Z + radius).ToLoc();

        for (var z = min.Z; z <= max.Z; z++)
        {
            for (var y = min.Y; y <= max.Y; y++)
            {
                for (var x = min.X; x <= max.X; x++)
                {
                    if (blocks.TryGet((x, y, z), out var block)
                        && block.IsSolid)
                        return true;
                }
            }
        }

        return false;
    }
}
