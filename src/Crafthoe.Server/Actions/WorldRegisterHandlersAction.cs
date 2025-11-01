namespace Crafthoe.Server;

[World]
public class WorldRegisterHandlersAction(
    NetLoop netLoop,
    NetEcho netEcho,
    WorldSpawnPlayerReceiver spawnPlayerReceiver,
    WorldMovePlayerReceiver movePlayerReceiver)
{
    public void Run()
    {
        netLoop.Register((int)CommonCommand.Echo, netEcho.Receive);
        netLoop.Register((int)ServerCommand.SpawnPlayer, spawnPlayerReceiver.Receive);
        netLoop.Register((int)ServerCommand.MovePlayer, movePlayerReceiver.Receive);
    }
}
