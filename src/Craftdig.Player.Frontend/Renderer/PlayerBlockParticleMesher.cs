namespace Craftdig.Player.Frontend;

[Player]
public class PlayerBlockParticleMesher(
    DimensionBlockParticleBag particles,
    DimensionBlockParticles blockParticles,
    DimensionLights lights,
    PlayerCamera camera)
{
    public void Mesh(List<BlockVertex> vertices, Vec3d origin)
    {
        var interpolation = (float)blockParticles.Alpha;
        var right = camera.Right;
        var up = camera.Up;

        foreach (var particle in particles.Ents)
        {
            var worldPosition = Vec3d.Lerp(
                particle.BlockParticlePrevPosition,
                particle.BlockParticlePosition,
                interpolation);
            var center =
                (Vec3)(worldPosition.Swizzle() - origin);
            var halfRight =
                right * (particle.BlockParticleSize * 0.5f);
            var halfUp =
                up * (particle.BlockParticleSize * 0.5f);
            var levels = lights.Get(worldPosition.ToLoc());
            var lighting = new Vec3(
                particle.BlockParticleBrightness,
                levels.Sky / (float)LightLevel.Max,
                levels.Block / (float)LightLevel.Max);
            var uvMin = particle.BlockParticleUvMin;
            var uvMax = particle.BlockParticleUvMax;
            var texture =
                particle.BlockParticleMaterial.Faces.Front.FaceIndex;

            vertices.Add(new(
                center - halfRight + halfUp,
                lighting,
                (uvMin.X, uvMax.Y, texture)));
            vertices.Add(new(
                center + halfRight + halfUp,
                lighting,
                (uvMax.X, uvMax.Y, texture)));
            vertices.Add(new(
                center - halfRight - halfUp,
                lighting,
                (uvMin.X, uvMin.Y, texture)));
            vertices.Add(new(
                center + halfRight - halfUp,
                lighting,
                (uvMax.X, uvMin.Y, texture)));
        }
    }
}
