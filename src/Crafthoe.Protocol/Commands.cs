namespace Crafthoe.Protocol;

public enum Commands : ushort
{
    CommonStart = 10000,
    Ping,
    Pong,

    ServerStart = 20000,
    SpawnPlayer,
    MovePlayer,
    ForgetChunk,

    ClientStart = 30000,
    ChunkUpdate,
    WorldIndicesUpdate,
    PositionUpdate,
}
