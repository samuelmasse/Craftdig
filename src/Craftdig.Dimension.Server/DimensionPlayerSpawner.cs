namespace Craftdig;

[Dimension]
public class DimensionPlayerSpawner(
    Log log,
    WorldPlayerProfiles profiles,
    WorldIndicesWrapper indicesWrapper,
    WorldPlayerSockets playerSockets,
    WorldPlayerSlots playerSlots,
    DimensionScope scope,
    DimensionEntArena entArena,
    DimensionEntIndex entIndex,
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

        if (ResolvePlayer(profileId, out var worldPlayer, out var player) is { } conflict)
        {
            log.Error("Refusing to spawn player {0}: {1}", ns.Tag, conflict);
            ns.Disconnect();
            return;
        }

        if (player.IsLoaded || player.IsSpawnReserved)
        {
            if (HasConnectedSession(worldPlayer))
            {
                log.Warn("Player ID {0} tried to connect more than once", profileId);
                ns.Disconnect();
                return;
            }

            queue.Enqueue((ns, profileId));
            return;
        }

        Prepare(ns, worldPlayer, player);
    }

    private bool HasConnectedSession(EntMutIdx worldPlayer)
    {
        foreach (var socket in playerSockets.Span)
        {
            if (socket.Connected && socket.SocketWorldPlayer == worldPlayer)
                return true;
        }

        return false;
    }

    private string? ResolvePlayer(Guid profileId, out EntMutIdx worldPlayer, out EntMutIdx player)
    {
        player = default;
        if (profiles.GetOrCreate(profileId, out worldPlayer, out bool created) is { } conflict)
            return conflict;

        return created ? Create(worldPlayer, out player) : Find(profileId, worldPlayer, out player);
    }

    private string? Find(Guid profileId, EntMutIdx worldPlayer, out EntMutIdx player)
    {
        if (playerIndex.TryGet(profileId, out player))
            return RequirePlayerId(profileId, player);

        if (playerIndex.IsDuplicated(profileId))
            return $"player ID {profileId} has multiple saved player Ents";

        var rloc = worldPlayer.WorldPosition.ToLoc().Xy.ToCloc().ToRloc();
        entRegionStates.EnsureLoaded(rloc);

        if (entIndex.IsDuplicated(profileId))
            return $"player ID {profileId} collides with multiple dimension Ents";
        if (!playerIndex.TryGet(profileId, out player))
            return $"player ID {profileId} has no dimension player Ent after loading its region";
        return RequirePlayerId(profileId, player);
    }

    private static string? RequirePlayerId(Guid profileId, EntMutIdx player) =>
        player.Id == profileId
            ? null
            : $"player {profileId} uses a pre-Identity dimension Ent ID; start this protocol with a clean world";

    private string? Create(EntMutIdx worldPlayer, out EntMutIdx player)
    {
        player = default;
        if (entIndex.Contains(worldPlayer.Id))
            return $"player ID {worldPlayer.Id} is already used by a dimension Ent";

        player = entArena.Alloc(worldPlayer.Id);
        player.HitBox = new Box3d((-0.3, -0.3, -1.62), (0.3, 0.3, 0.18));
        player.Position = (15, 0, 120);
        player.LookAt = (0, -1, 0);
        player.IsRigid = true;
        player.IsFlying = true;
        player.IsPlayer = true;
        return null;
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
