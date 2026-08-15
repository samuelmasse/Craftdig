namespace Craftdig;

public readonly record struct PlayerChunkLightUpdate(
    Vec2i Cloc,
    ChunkLight Light,
    uint SectionMask,
    bool Full);
