namespace Craftdig;

[Player]
public class PlayerEntReplicas(
    WorldEntArena worldArena,
    DimensionEntArena dimensionArena,
    DimensionChunks chunks,
    PlayerIdentityCache identityCache,
    PlayerEnt player)
{
    private readonly Dictionary<Guid, PlayerEntReplica> replicasById = [];
    private readonly Dictionary<Vec2i, HashSet<Guid>> replicaIdsByChunk = [];

    public bool OwnerReady;

    public PlayerEntReplica this[Guid id] => replicasById[id];

    public bool Contains(Guid id) => replicasById.ContainsKey(id);

    public bool IsOwner(Guid id) => replicasById[id].IsOwner;

    public EntMutIdx Ent(Guid id) => replicasById[id].Ent;

    public void Create(uint scopeId, Guid id, bool isOwner)
    {
        PlayerEntReplica replica;
        if (scopeId == 0)
            replica = AllocateWorldReplica(id, isOwner);
        else if (isOwner)
            replica = BindOwnedDimensionReplica(id);
        else replica = AllocateRemoteDimensionReplica(id);

        replicasById.Add(id, replica);
    }

    public void Delete(Guid id)
    {
        var replica = replicasById[id];
        replicasById.Remove(id);

        if (replica.Ent.IsPlayer)
            identityCache.ForgetPlayerEnt(id);

        if (replica.Chunk != null)
            RemoveFromChunk(replica.Chunk.Value, id);

        if (replica.IsBoundToPlayer)
        {
            // The player scope owns PlayerEnt; Ent synchronization only borrowed it.
            SetLoaded(replica.Ent, false);
        }
        else replica.Allocation.Dispose();
    }

    public void MarkOwnerReady(EntMutIdx ent)
    {
        SetLoaded(ent, true);
        OwnerReady = true;
    }

    public void UpdateRemoteChunkMembership(Guid id, ref PlayerEntReplica replica)
    {
        // Chunk membership follows the authoritative position, not its interpolated visual
        // position, so streaming and lifetime decisions agree with the server immediately.
        Vec2i? currentChunk = replica.Ent.Has<Vec3d, DimensionComponents.Position>()
            ? replica.Ent.Position.ToLoc().Xy.ToCloc()
            : null;

        if (replica.Chunk != currentChunk)
        {
            if (replica.Chunk != null)
                RemoveFromChunk(replica.Chunk.Value, id);
            if (currentChunk != null)
                AddToChunk(currentChunk.Value, id);

            replica.Chunk = currentChunk;
        }

        bool chunkLoaded = currentChunk != null &&
            chunks.TryGet(currentChunk.Value, out var chunk) &&
            chunk.IsLoaded;
        SetLoaded(replica.Ent, chunkLoaded);
        replicasById[id] = replica;
    }

    public void ChunkLoaded(Vec2i chunk) => SetChunkLoaded(chunk, true);

    public void ChunkUnloaded(Vec2i chunk) => SetChunkLoaded(chunk, false);

    private PlayerEntReplica BindOwnedDimensionReplica(Guid id)
    {
        // Player systems already hold the injected PlayerEnt. Bind the server's persistent Id
        // to that Ent instead of allocating a second player no other player system uses.
        EntMutIdx ent = player;
        ent.Id = id;
        return new(ent, default, true, true);
    }

    private PlayerEntReplica AllocateRemoteDimensionReplica(Guid id)
    {
        var allocation = dimensionArena.Alloc(id);
        EntMutIdx ent = allocation;
        ent.IsRemote = true;
        return new(ent, allocation, false, false);
    }

    private PlayerEntReplica AllocateWorldReplica(Guid id, bool isOwner)
    {
        var allocation = worldArena.Alloc(id);
        EntMutIdx ent = allocation;
        ent.IsLoaded = true;
        return new(ent, allocation, isOwner, false);
    }

    private void SetChunkLoaded(Vec2i chunk, bool loaded)
    {
        if (!replicaIdsByChunk.TryGetValue(chunk, out var ids))
            return;

        foreach (Guid id in ids)
            SetLoaded(replicasById[id].Ent, loaded);
    }

    private void AddToChunk(Vec2i chunk, Guid id)
    {
        if (!replicaIdsByChunk.TryGetValue(chunk, out var ids))
        {
            ids = [];
            replicaIdsByChunk.Add(chunk, ids);
        }

        ids.Add(id);
    }

    private void RemoveFromChunk(Vec2i chunk, Guid id)
    {
        var ids = replicaIdsByChunk[chunk];
        ids.Remove(id);
        if (ids.Count == 0)
            replicaIdsByChunk.Remove(chunk);
    }

    private void SetLoaded(EntMutIdx ent, bool loaded)
    {
        if (ent.IsLoaded != loaded)
            ent.IsLoaded = loaded;
    }
}
