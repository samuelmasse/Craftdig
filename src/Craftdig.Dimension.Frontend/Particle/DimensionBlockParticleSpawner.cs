namespace Craftdig;

[Dimension]
public class DimensionBlockParticleSpawner(
    DimensionEntArena arena,
    DimensionBlockParticleBag particles)
{
    private const int Subdivisions = 4;
    private const int MaximumParticles = 1024;
    private const double CellPositionJitter = 0.28;
    private const double DirectionJitter = 0.55;

    private readonly Random random = new();

    public void Spawn(Vec3i location, Ent material)
    {
        var origin = new Vec3d(location.X, location.Y, location.Z);
        for (var z = 0; z < Subdivisions; z++)
        {
            for (var y = 0; y < Subdivisions; y++)
            {
                for (var x = 0; x < Subdivisions; x++)
                    SpawnCell(origin, material, x, y, z);
            }
        }
    }

    private void SpawnCell(
        Vec3d origin,
        Ent material,
        int x,
        int y,
        int z)
    {
        MakeRoom();

        var fx = CellPosition(x);
        var fy = CellPosition(y);
        var fz = CellPosition(z);
        var position = origin + new Vec3d(fx, fy, fz);
        var velocity = Velocity(fx, fy, fz);
        var uvMin = new Vec2(
            random.Next(Subdivisions) / (float)Subdivisions,
            random.Next(Subdivisions) / (float)Subdivisions);
        const float uvInset = 0.004f;
        uvMin += new Vec2(uvInset);
        var uvMax =
            uvMin
            + new Vec2(1f / Subdivisions - uvInset * 2);
        var maxAge = Math.Max(
            4,
            (int)(4.0 / (random.NextDouble() * 0.9 + 0.1)));

        var allocation = arena.AllocTransient();
        allocation.Mutate()
            .BlockParticleAllocation(allocation)
            .BlockParticleMaterial(material)
            .BlockParticlePosition(position)
            .BlockParticlePrevPosition(position)
            .BlockParticleVelocity(velocity)
            .BlockParticleSize(
                0.105f + (float)random.NextDouble() * 0.045f)
            .BlockParticleBrightness(
                0.72f + (float)random.NextDouble() * 0.28f)
            .BlockParticleAge(0)
            .BlockParticleMaxAge(maxAge)
            .BlockParticleUvMin(uvMin)
            .BlockParticleUvMax(uvMax)
            .IsBlockParticle(true);
    }

    private Vec3d Velocity(double fx, double fy, double fz)
    {
        var radial = new Vec3d(
            fx - 0.5,
            fy - 0.5,
            fz - 0.5);
        var divisor = Math.Max(
            Math.Abs(radial.X),
            Math.Max(Math.Abs(radial.Y), Math.Abs(radial.Z)));
        var direction =
            radial / divisor
            + new Vec3d(
                Jitter(DirectionJitter),
                Jitter(DirectionJitter),
                Jitter(DirectionJitter));
        var length = Math.Sqrt(
            direction.X * direction.X
            + direction.Y * direction.Y
            + direction.Z * direction.Z);
        var speed =
            0.035
            + (random.NextDouble() + random.NextDouble()) * 0.035;
        var lift = 0.008 + random.NextDouble() * 0.018;
        return direction / length * speed
            + new Vec3d(0, 0, lift);
    }

    private double CellPosition(int index) =>
        (index + 0.5 + Jitter(CellPositionJitter))
        / Subdivisions;

    private void MakeRoom()
    {
        while (particles.Count >= MaximumParticles)
            particles.Ents[0].BlockParticleAllocation.Dispose();
    }

    private double Jitter(double magnitude) =>
        (random.NextDouble() * 2.0 - 1.0) * magnitude;
}
