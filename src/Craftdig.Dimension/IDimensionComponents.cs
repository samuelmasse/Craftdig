namespace Craftdig.Dimension;

[Components]
public interface IDimensionComponents
{
    // Rigid
    [Saved][ComponentToString] bool IsRigid { get; set; }
    [Saved] Vector3d Position { get; set; }
    [Saved] Vector3d Velocity { get; set; }
    [Saved] Box3d HitBox { get; set; }

    // Player
    [Saved][ComponentToString] bool IsPlayer { get; set; }
    [Saved] Ent[]? ArmorSlotEnts { get; set; }
    [Saved] int[]? ArmorSlotCounts { get; set; }
    [Saved] Ent[]? InventorySlotEnts { get; set; }
    [Saved] int[]? InventorySlotCounts { get; set; }
    [Saved] Ent[]? HotBarSlotEnts { get; set; }
    [Saved] int[]? HotBarSlotCounts { get; set; }
    [Saved] Ent OffhandEnt { get; set; }
    [Saved] int OffhandCount { get; set; }
    [Saved] int HotBarIndex { get; set; }
    [Saved] bool IsFlying { get; set; }
    [Saved] bool IsSprinting { get; set; }

    // Test cube
    [Saved][ComponentToString] bool IsTestCube { get; set; }
    [Saved] Ent TestCubeMaterial { get; set; }
    [Saved] float TestCubeSize { get; set; }
    [Saved] bool IsProjectile { get; set; }

    // Chunk
    [ComponentToString] bool IsChunk { get; set; }
    [ComponentToString] Vector2i Cloc { get; set; }
    ChunkBlocks? ChunkBlocks { get; set; }
    HashSet<Ent>? ChunkRigids { get; set; }

    // Rigid
    Vector3d PrevPosition { get; set; }
    Vector3i CollisionNormal { get; set; }
    Vector2i? RigidCloc { get; set; }

    // Playera action state
    MovementStep Movement { get; set; }
    ConstructionStep Construction { get; set; }

    // Player selection state
    BlockSelection? BlockSelection { get; set; }
    Vector3d BlockSelectionPosition { get; set; }
    Vector3 BlockSelectionLookAt { get; set; }
    long BlockSelectionLastComputed { get; set; }

    // Player movement permissions
    bool CanFly { get; set; }
    bool CanSprint { get; set; }
    bool CanJump { get; set; }
    bool CanMove { get; set; }
    bool CanMoveVertically { get; set; }

    // Dimension scope
    DimensionScope DimensionScope { get; set; }
}
