namespace Crafthoe.Server;

[Server]
public class ServerRegisterHandlersAction(
    ServerNetLoop loop,
    ServerPingReceiver pingReceiver,
    ServerSpawnPlayerReceiver spawnPlayerReceiver)
{
    public void Run()
    {
        loop.Register<PingCommand>(
            (int)CommonCommand.Ping, pingReceiver.Receive);

        loop.Register((int)ServerCommand.SpawnPlayer,
            spawnPlayerReceiver.Receive);

        loop.Register((int)ServerCommand.MovePlayer,
            DimensionHandler<DimensionMovePlayerReceiver, MovePlayerCommand>());

        loop.Register((int)ServerCommand.ForgetChunk,
            DimensionHandler<DimensionForgetChunkReceiver, ForgetChunkCommand>());
    }

    private Action<NetSocket, C> DimensionHandler<T, C>()
        where T : DimensionReceiver<C> where C : unmanaged => (ns, cmd) =>
    {
        if (ns.Ent.SocketPlayer() == null)
        {
            ns.Disconnect();
            return;
        }

        ns.Ent.DimensionScope().Get<T>().Receive(ns, cmd);
    };
}
