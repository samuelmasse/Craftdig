namespace Crafthoe.Server;

[Server]
public class ServerRegisterHandlersAction(
    ServerNetLoop loop,
    ServerPingReceiver pingReceiver,
    ServerSpawnPlayerReceiver spawnPlayerReceiver)
{
    public void Run()
    {
        loop.Register<PingCommand>(pingReceiver.Receive);
        loop.Register<SpawnPlayerCommand>(spawnPlayerReceiver.Receive);
        loop.Register(DimensionHandler<DimensionMovePlayerReceiver, MovePlayerCommand>());
        loop.Register(DimensionHandler<DimensionForgetChunkReceiver, ForgetChunkCommand>());
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
