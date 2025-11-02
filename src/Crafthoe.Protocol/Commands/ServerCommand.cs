namespace Crafthoe.Protocol;

public enum ServerCommand : int
{
    First = 0x3000000,
    SpawnPlayer,
    MovePlayer,
    ForgetChunk
}
