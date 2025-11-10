namespace Crafthoe.Protocol;

public enum Commands
{
    CommonStart = 1000000,
    Ping,
    Pong,

    ServerStart = 2000000,
    SpawnPlayer,
    MovePlayer,
    ForgetChunk,

    ClientStart = 3000000,
    ChunkUpdate,
    WorldIndicesUpdate,
    PositionUpdate,
}
