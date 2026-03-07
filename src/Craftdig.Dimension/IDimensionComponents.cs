namespace Craftdig.Dimension;

[Components]
public interface IDimensionComponents
{
    // Context
    HashSet<EntPtrIdx> ContextEntLiveSet { get; set; }

    DimensionScope DimensionScope { get; set; }

    // Chunk
    [ComponentToString] bool IsChunk { get; set; }
    [ComponentToString] Vector2i Cloc { get; set; }
    ChunkBlocks? ChunkBlocks { get; set; }
    HashSet<Ent>? ChunkRigids { get; set; }

    // Rigid
    [ComponentToString] bool IsRigid { get; set; }
    Vector2i? RigidCloc { get; set; }
    Vector3d Position { get; set; }
    Vector3d PrevPosition { get; set; }
    Vector3d Velocity { get; set; }
    Vector3i CollisionNormal { get; set; }
    Box3d HitBox { get; set; }
    bool IsFlying { get; set; }
    bool IsRunning { get; set; }
    bool IsSprinting { get; set; }
    MovementStep Movement { get; set; }
    ConstructionStep Construction { get; set; }
    bool CanFly { get; set; }
    bool CanSprint { get; set; }
    bool CanJump { get; set; }
    bool CanMove { get; set; }
    bool CanMoveVertically { get; set; }

    // Player
    bool IsPlayer { get; set; }
    long BlockSelectionLastComputed { get; set; }
    BlockSelection? BlockSelection { get; set; }
    Vector3d BlockSelectionPosition { get; set; }
    Vector3 BlockSelectionLookAt { get; set; }

    // TestCube
    [ComponentToString] bool IsTestCube { get; set; }
    Ent TestCubeMaterial { get; set; }
    float TestCubeSize { get; set; }
    bool IsProjectile { get; set; }
}
