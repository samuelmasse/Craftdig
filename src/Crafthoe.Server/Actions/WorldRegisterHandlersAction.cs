namespace Crafthoe.Server;

[World]
public class WorldRegisterHandlersAction(
    NetLoop netLoop,
    NetEcho netEcho,
    WorldSpawnPlayerReceiver spawnPlayerReceiver)
{
    public void Run()
    {
        netLoop.Register((int)CommonCommand.Echo, netEcho.Receive);
        netLoop.Register((int)ServerCommand.SpawnPlayer, spawnPlayerReceiver.Receive);
        netLoop.Register((int)ServerCommand.MovePlayer, DimensionHandler<DimensionMovePlayerReceiver>());
        netLoop.Register((int)ServerCommand.ForgetChunk, DimensionHandler<DimensionForgetChunkReceiver>());
    }

    private Action<NetSocket, NetMessage> DimensionHandler<T>() where T : DimensionReceiver => (ns, msg) =>
    {
        if (ns.Ent.SocketPlayer() == null)
        {
            ns.Disconnect();
            return;
        }

        ns.Ent.DimensionScope().Get<T>().Receive(ns, msg);
    };
}
