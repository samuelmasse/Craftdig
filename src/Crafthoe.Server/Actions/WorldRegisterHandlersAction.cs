namespace Crafthoe.Server;

[World]
public class WorldRegisterHandlersAction(NetLoop netLoop, NetEcho netEcho, WorldSpawnReceiver spawnReceiver)
{
    public void Run()
    {
        netLoop.Register((int)CommonCommand.Echo, netEcho.Receive);
        netLoop.Register((int)ServerCommand.Spawn, spawnReceiver.Receive);
    }
}
