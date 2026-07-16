namespace Craftdig.Dimension.Server;

[Dimension]
public class DimensionPlayerSpawner(
    AppLog log,
    WorldPlayerProfiles profiles,
    WorldIndicesWrapper indicesWrapper,
    WorldPlayerSockets playerSockets,
    WorldPlayerSlots playerSlots,
    DimensionScope scope,
    DimensionEntArena entArena,
    DimensionPlayerIndex playerIndex,
    DimensionEntRegionStates entRegionStates,
    DimensionSockets sockets)
{
    private readonly ConcurrentQueue<(NetSocket Socket, Guid ProfileId)> queue = [];

    public void Add(NetSocket ns, Guid profileId) => queue.Enqueue((ns, profileId));

    public void Tick()
    {
        int count = queue.Count;

        while (count > 0 && queue.TryDequeue(out var request))
        {
            Spawn(request.Socket, request.ProfileId);
            count--;
        }
    }

    private void Spawn(NetSocket ns, Guid profileId)
    {
        if (ns.SocketWorldPlayer != default)
        {
            log.Warn("Player {0} tried to spawn again", ns.Tag);
            ns.Disconnect();
            return;
        }

        var worldPlayer = profiles.GetOrCreate(profileId, out bool created);
        EntMutIdx player;

        if (created)
            player = Create(worldPlayer);
        else
        {
            player = Find(profileId, worldPlayer);
            if (player.IsLoaded || player.IsSpawnReserved)
            {
                queue.Enqueue((ns, profileId));
                return;
            }
        }

        Prepare(ns, worldPlayer, player);
    }

    private EntMutIdx Find(Guid profileId, EntMutIdx worldPlayer)
    {
        if (playerIndex.TryGet(profileId, out var player))
            return player;

        var rloc = worldPlayer.WorldPosition.ToLoc().Xy.ToCloc().ToRloc();
        entRegionStates.EnsureLoaded(rloc);
        return playerIndex[profileId];
    }

    private EntMutIdx Create(EntMutIdx worldPlayer)
    {
        var player = entArena.Alloc();
        player.HitBox = new Box3d((-0.3, -0.3, -1.62), (0.3, 0.3, 0.18));
        player.Position = (15, 0, 120);
        player.LookAt = (0, -1, 0);
        player.WorldPlayer = worldPlayer;
        player.IsRigid = true;
        player.IsFlying = true;
        player.IsPlayer = true;
        return player;
    }

    private void Prepare(NetSocket ns, EntMutIdx worldPlayer, EntMutIdx player)
    {
        const byte terrainSideLength = 5;

        log.Info("Player {0} is loading terrain", ns.Tag);

        if (player.LookAt == default)
            player.LookAt = (0, -1, 0);

        worldPlayer.Tag = ns.Tag;
        player.Tag = ns.Tag;
        player.CanFly = true;
        player.CanMove = true;
        player.CanSprint = true;
        player.CanJump = true;
        player.PrevPosition = player.Position;
        player.CollisionNormal = default;
        player.Movement = default;
        player.Construction = default;
        player.Drop = default;
        player.PendingMovement = [];
        player.PendingMovementWait = 0;
        ns.LastInventoryActionSequence = 0;
        ns.LastInventoryActionStatus = default;
        ns.LastInventoryActionRevision = player.InventoryRevision;

        player.IsSpawnReserved = true;
        ns.SocketWorldPlayer = worldPlayer;
        ns.SocketPlayer = player;
        ns.DimensionScope = scope;
        ns.PlayerSlot = playerSlots.Take();
        ns.PlayerSpawnPhase = PlayerSpawnPhase.LoadingTerrain;
        ns.TerrainLoadCenter = player.Position.ToLoc().Xy.ToCloc();
        ns.TerrainLoadSideLength = terrainSideLength;
        ns.SocketStreamedChunks = [];

        sockets.Add(ns);
        playerSockets.Add(ns);
        player.IsSeer = true;
        player.IsLoaded = false;
        ns.Send<WorldIndicesUpdateCommand, byte>(indicesWrapper.Wrap());
        ns.Send(new BeginTerrainLoadCommand
        {
            CenterCloc = ns.TerrainLoadCenter,
            SideLength = ns.TerrainLoadSideLength,
        });
    }

    public void Activate(NetSocket ns)
    {
        if (!ns.Connected || ns.PlayerSpawnPhase != PlayerSpawnPhase.LoadingTerrain)
            return;

        ns.PlayerSpawnPhase = PlayerSpawnPhase.Active;
        var player = ns.SocketPlayer;
        player.IsLoaded = true;
        log.Info("Player {0} finished loading terrain and spawned", ns.Tag);
    }
}
