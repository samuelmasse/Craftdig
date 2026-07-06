namespace Craftdig.Dimension;

[Components]
public interface IDimensionComponents
{
    // Rigid
    [Saved][ComponentToString] bool IsRigid { get; set; }
    [Saved] Vec3d Position { get; set; }
    [Saved] Vec3d Velocity { get; set; }
    [Saved] Box3d HitBox { get; set; }

    // Player
    [Saved][ComponentToString] bool IsPlayer { get; set; }
    [Saved] EntMutIdx WorldPlayer { get; set; }
    [Saved] Vec3 LookAt { get; set; }
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
    [ComponentToString] Vec2i Cloc { get; set; }
    ChunkBlocks? ChunkBlocks { get; set; }

    // Rigid
    Vec3d PrevPosition { get; set; }
    Vec3i CollisionNormal { get; set; }
    Vec2i? RigidCloc { get; set; }

    // Playera action state
    MovementStep Movement { get; set; }
    ConstructionStep Construction { get; set; }
    DropStep Drop { get; set; }

    // Player selection state
    BlockSelection? BlockSelection { get; set; }
    Vec3d BlockSelectionPosition { get; set; }
    long BlockSelectionLastComputed { get; set; }

    // Player movement permissions
    bool CanFly { get; set; }
    bool CanSprint { get; set; }
    bool CanJump { get; set; }
    bool CanMove { get; set; }
    bool CanMoveVertically { get; set; }

    // Controls chunk loading
    bool IsSeer { get; set; }

    // Dimension scope
    DimensionScope DimensionScope { get; set; }
}
