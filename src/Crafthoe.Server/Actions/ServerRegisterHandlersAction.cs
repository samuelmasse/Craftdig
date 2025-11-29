namespace Crafthoe.Server;

[Server]
public class ServerRegisterHandlersAction(
    AppLog log,
    ServerNetLoop loop,
    ServerDefaults defaults,
    ServerAuthReceiver authReceiver,
    ServerNoAuthReceiver noAuthReceiver,
    ServerPingReceiver pingReceiver,
    ServerSpawnPlayerReceiver spawnPlayerReceiver)
{
    public void Run()
    {
        loop.Register<AuthCommand, byte>(authReceiver.Receive);
        if (defaults.NoAuth)
            loop.Register<NoAuthCommand, byte>(noAuthReceiver.Receive);
        loop.Register(Authenticated<PingCommand>(pingReceiver.Receive));
        loop.Register(Authenticated<SpawnPlayerCommand>(spawnPlayerReceiver.Receive));
        loop.Register(Authenticated(DimensionHandler<DimensionMovePlayerReceiver, MovePlayerCommand>()));
        loop.Register(Authenticated(DimensionHandler<DimensionForgetChunkReceiver, ForgetChunkCommand>()));
        loop.Register(Authenticated(DimensionHandler<DimensionForgetSectionReceiver, ForgetSectionCommand>()));
    }

    private Action<NetSocket, C> Authenticated<C>(Action<NetSocket, C> handler) where C : unmanaged => (ns, cmd) =>
    {
        if (!ns.Ent.IsAuthenticated())
        {
            log.Warn("Socket {0} tried to perform an authenticated action before authenticating", ns.Ent.Tag());
            ns.Disconnect();
            return;
        }

        handler(ns, cmd);
    };

    private Action<NetSocket, C> Authenticated<C>(Action<NetSocket> handler) where C : unmanaged =>
        Authenticated<C>((ns, cmd) => handler(ns));

    private Action<NetSocket, C> DimensionHandler<T, C>()
        where T : DimensionReceiver<C> where C : unmanaged => (ns, cmd) =>
    {
        if (ns.Ent.SocketPlayer() == null)
        {
            log.Warn("Socket {0} tried to perform a dimension action before spawning", ns.Ent.Tag());
            ns.Disconnect();
            return;
        }

        ns.Ent.DimensionScope().Get<T>().Receive(ns, cmd);
    };
}
