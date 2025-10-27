namespace Crafthoe.Server;

[World]
public class WorldRegisterHandlersAction(NetLoop netLoop, NetEcho netEcho, WorldSpawnReceiver spawnReceiver)
{
    public void Run()
    {
        netLoop.Register(NetEcho.Type, netEcho.Receive);
        netLoop.Register(WorldSpawnWrapper.Type, spawnReceiver.Receive);
    }
}
