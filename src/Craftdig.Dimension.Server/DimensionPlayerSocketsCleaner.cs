namespace Craftdig;

[Dimension]
public class DimensionPlayerSocketsCleaner(
    Log log,
    WorldPlayerSockets worldSockets,
    WorldPlayerSlots playerSlots,
    DimensionSockets sockets)
{
    private readonly List<NetSocket> remove = [];

    public void Tick()
    {
        foreach (var ns in sockets.Span)
        {
            if (!ns.Connected)
                remove.Add(ns);
        }

        foreach (var ns in remove)
        {
            var tag = ns.SocketWorldPlayer.Tag;
            var player = ns.SocketPlayer;

            player.IsSeer = false;
            player.IsLoaded = false;
            player.IsSpawnReserved = false;
            player.PendingMovement = null;
            player.PendingMovementWait = 0;
            player.Movement = default;
            player.Construction = default;
            player.Drop = default;

            ns.PlayerSpawnPhase = PlayerSpawnPhase.None;
            ns.TerrainLoadCenter = default;
            ns.TerrainLoadSideLength = 0;
            ns.SocketStreamedChunks?.Clear();

            playerSlots.Return(ns.PlayerSlot);

            worldSockets.Remove(ns);
            sockets.Remove(ns);

            log.Info("Player {0} left", tag);
        }

        remove.Clear();
    }
}
