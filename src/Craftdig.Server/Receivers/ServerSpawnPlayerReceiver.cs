namespace Craftdig.Server;

[Server]
public class ServerSpawnPlayerReceiver(AppLog log, WorldEntArena entArena, ServerPlayerSpawner playerSpawner)
{
    public void Receive(NetSocket ns)
    {
        if (ns.SocketWorldPlayer != default)
        {
            log.Warn("Player {0} tried to spawn again", ns.Tag);
            ns.Disconnect();
            return;
        }

        log.Info("Player {0} requested to spawn", ns.Tag);

        var worldPlayer = entArena.Alloc();
        worldPlayer.Tag = ns.Tag;
        ns.SocketWorldPlayer = worldPlayer;

        playerSpawner.Add(ns);
    }
}
