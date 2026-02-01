namespace Craftdig.Protocol;

public enum Commands : ushort
{
    CommonStart = 10000,
    Ping,
    Pong,

    ServerStart = 20000,
    BeginAuth,
    NoAuth,
    SpawnPlayer,
    MovePlayer,
    ForgetChunk,
    ForgetSection,

    ClientStart = 30000,
    ReadyAuth,
    ChunkUpdate,
    WorldIndicesUpdate,
    PositionUpdate,
    SlowDown,
    SlowTick,
    SectionUpdate
}
