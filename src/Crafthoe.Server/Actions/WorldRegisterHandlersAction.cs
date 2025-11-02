namespace Crafthoe.Server;

[World]
public class WorldRegisterHandlersAction(
    NetLoop netLoop,
    NetEcho netEcho,
    WorldSpawnPlayerReceiver spawnPlayerReceiver,
    WorldMovePlayerReceiver movePlayerReceiver,
    WorldForgetChunkReceiver forgetChunkReceiver)
{
    public void Run()
    {
        netLoop.Register((int)CommonCommand.Echo, netEcho.Receive);
        netLoop.Register((int)ServerCommand.SpawnPlayer, spawnPlayerReceiver.Receive);
        netLoop.Register((int)ServerCommand.MovePlayer, movePlayerReceiver.Receive);
        netLoop.Register((int)ServerCommand.ForgetChunk, forgetChunkReceiver.Receive);
    }
}
