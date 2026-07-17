namespace Craftdig.Dimension;

[Components]
public interface IDimensionComponents
{
    // Rigid
    [Saved][Synced][ComponentToString] bool IsRigid { get; set; }
    [Saved][Synced(EntSyncAudience.Observers)] Vec3d Position { get; set; }
    [Saved] Vec3d Velocity { get; set; }
    [Saved][Synced] Box3d HitBox { get; set; }

    // Player
    [Saved][Synced][ComponentToString] bool IsPlayer { get; set; }
    [Saved] EntMutIdx WorldPlayer { get; set; }
    [Saved][Synced(EntSyncAudience.Observers)] Vec3 LookAt { get; set; }
    [Saved][Synced(EntSyncAudience.Owner, Slots.ArmorCount)] Ent[]? ArmorSlotEnts { get; set; }
    [Saved][Synced(EntSyncAudience.Owner, Slots.ArmorCount)] int[]? ArmorSlotCounts { get; set; }
    [Saved][Synced(EntSyncAudience.Owner, Slots.InventoryCount)] Ent[]? InventorySlotEnts { get; set; }
    [Saved][Synced(EntSyncAudience.Owner, Slots.InventoryCount)] int[]? InventorySlotCounts { get; set; }
    [Saved][Synced(EntSyncAudience.Owner, Slots.HotBarCount)] Ent[]? HotBarSlotEnts { get; set; }
    [Saved][Synced(EntSyncAudience.Owner, Slots.HotBarCount)] int[]? HotBarSlotCounts { get; set; }
    [Saved][Synced(EntSyncAudience.Owner)] Ent OffhandEnt { get; set; }
    [Saved][Synced(EntSyncAudience.Owner)] int OffhandCount { get; set; }
    [Saved][Synced(EntSyncAudience.Owner)] int HotBarIndex { get; set; }
    [Saved][Synced(EntSyncAudience.Owner)] long InventoryRevision { get; set; }
    [Saved] bool IsFlying { get; set; }
    [Saved] bool IsSprinting { get; set; }
    [Synced(EntSyncAudience.Observers)] bool IsCrouching { get; set; }

    // Test cube
    [Saved][Synced][ComponentToString] bool IsTestCube { get; set; }
    [Saved][Synced] Ent TestCubeMaterial { get; set; }
    [Saved][Synced] float TestCubeSize { get; set; }
    [Saved] bool IsProjectile { get; set; }

    // Chunk
    [ComponentToString] bool IsChunk { get; set; }
    [ComponentToString] Vec2i Cloc { get; set; }
    ChunkBlocks? ChunkBlocks { get; set; }
    ChunkLight? ChunkLight { get; set; }
    bool IsLightReady { get; set; }

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
    [Synced(EntSyncAudience.Owner)] bool CanFly { get; set; }
    [Synced(EntSyncAudience.Owner)] bool CanSprint { get; set; }
    [Synced(EntSyncAudience.Owner)] bool CanJump { get; set; }
    [Synced(EntSyncAudience.Owner)] bool CanMove { get; set; }
    [Synced(EntSyncAudience.Owner)] bool CanMoveVertically { get; set; }

    // Networking
    bool IsRemote { get; set; }

    // Controls chunk loading
    bool IsSeer { get; set; }

    // Dimension scope
    DimensionScope DimensionScope { get; set; }
}
