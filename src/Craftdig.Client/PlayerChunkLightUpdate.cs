namespace Craftdig.Client;

public readonly record struct PlayerChunkLightUpdate(
    Vec2i Cloc,
    ChunkLight Light,
    uint SectionMask,
    bool Full);
