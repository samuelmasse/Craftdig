namespace Craftdig.Dimension;

[Components]
file record DimensionComponents(
    DimensionScope DimensionScope,

    // Chunk
    [ComponentToString] bool IsChunk,
    [ComponentToString] Vector2i Cloc,
    ChunkBlocks? ChunkBlocks,
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
    ConstructionStep Construction,
    bool CanFly,
    bool CanSprint,
    bool CanJump,
    bool CanMove,
    bool CanMoveVertically,

    // Player
    int PlayerBagIndex,
    long BlockSelectionLastComputed,
    BlockSelection? BlockSelection,
    Vector3d BlockSelectionPosition,
    Vector3 BlockSelectionLookAt,

    // TestCube
    [ComponentToString] bool IsTestCube,
    Ent TestCubeMaterial,
    float TestCubeSize,
    bool IsProjectile
);
