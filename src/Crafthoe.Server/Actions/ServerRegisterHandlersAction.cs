namespace Crafthoe.Server;

[Server]
public class ServerRegisterHandlersAction(
    NetLoop netLoop,
    NetEcho netEcho,
    ServerPingReceiver pingReceiver,
    ServerSpawnPlayerReceiver spawnPlayerReceiver)
{
    public void Run()
    {
        netLoop.Register((int)CommonCommand.Echo, netEcho.Receive);
        netLoop.Register((int)CommonCommand.Ping, pingReceiver.Receive);
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
