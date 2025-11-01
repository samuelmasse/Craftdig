namespace Crafthoe.Dimension;

[Components]
file record DimensionComponents(
    DimensionScope DimensionScope,

    // Chunk
    [ComponentToString] bool IsChunk,
    [ComponentToString] Vector2i Cloc,
    [ComponentReturnSpan] Memory<Ent> Blocks,
    int ChunkBagIndex,

    // Rigid
    Vector3d Position,
    Vector3d PrevPosition,
    Vector3d Velocity,
    Vector3i CollisionNormal,
    Box3d HitBox,
    [ComponentToString] bool IsRigid,
    bool IsFlying,
    bool IsRunning,
    bool IsSprinting,
    int RigidBagIndex,
    MovementStep Movement,

    // Player
    int PlayerBagIndex
);
