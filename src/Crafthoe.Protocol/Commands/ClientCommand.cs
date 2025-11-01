namespace Crafthoe.Protocol;

public enum ClientCommand : int
{
    First = 0x2000000,
    ChunkUpdate,
    WorldIndicesUpdate,
    PositionUpdate
}
