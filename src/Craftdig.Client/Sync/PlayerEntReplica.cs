namespace Craftdig.Client;

public struct PlayerEntReplica(
    EntMutIdx ent,
    EntPtrIdx allocation,
    bool isOwner,
    bool isBoundToPlayer)
{
    public readonly EntMutIdx Ent = ent;
    public readonly EntPtrIdx Allocation = allocation;
    public readonly bool IsOwner = isOwner;
    public readonly bool IsBoundToPlayer = isBoundToPlayer;
    public Vec2i? Chunk;
}
