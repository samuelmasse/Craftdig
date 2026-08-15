namespace Craftdig;

[Components]
public interface IDimensionFrontendComponents
{
    // Block particle
    [ComponentToString] bool IsBlockParticle { get; set; }
    EntPtrIdx BlockParticleAllocation { get; set; }
    [ComponentToString] Ent BlockParticleMaterial { get; set; }
    Vec3d BlockParticlePosition { get; set; }
    Vec3d BlockParticlePrevPosition { get; set; }
    Vec3d BlockParticleVelocity { get; set; }
    float BlockParticleSize { get; set; }
    float BlockParticleBrightness { get; set; }
    int BlockParticleAge { get; set; }
    int BlockParticleMaxAge { get; set; }
    Vec2 BlockParticleUvMin { get; set; }
    Vec2 BlockParticleUvMax { get; set; }

    // Remote rigid presentation
    RemotePositionInterpolation RemotePosition { get; set; }
    RemoteLookAtInterpolation RemoteLookAt { get; set; }

    // Chunk
    Memory<EntPtrIdx> Sections { get; set; }
    SortedList<int, int> Unrendered { get; set; }
    bool IsUnrenderedListBuilt { get; set; }
    bool IsReadyToRender { get; set; }
    SortedList<int, int> Rendered { get; set; }

    // Section
    [ComponentToString] bool IsSection { get; set; }
    [ComponentToString] Vec3i Sloc { get; set; }
    int MeshRevision { get; set; }
    bool IsMeshPending { get; set; }
    bool IsMeshDirty { get; set; }
    SectionMesh TerrainMesh { get; set; }
    EntMutIdx Chunk { get; set; }
}
