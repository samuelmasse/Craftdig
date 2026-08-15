namespace Craftdig;

[Server]
public class ServerRegisterHandlersAction(
    Log log,
    ServerNetLoop loop,
    ServerDefaults defaults,
    ServerAuth auth,
    ServerAuthReceiver authReceiver,
    ServerPresenceReceiver presenceReceiver,
    ServerPingReceiver pingReceiver,
    ServerStatusReceiver statusReceiver,
    ServerSpawnPlayerReceiver spawnPlayerReceiver)
{
    public void Run()
    {
        loop.Register<PingCommand>(pingReceiver.Receive);
        loop.Register<ServerStatusCommand>(statusReceiver.Receive);

        loop.RegisterRaw<BeginAuthCommand>(authReceiver.BeginAuth);
        loop.RegisterRaw<CompleteAuthCommand>(authReceiver.CompleteAuth);
        if (defaults.NoAuth)
            loop.Register<NoAuthCommand, byte>(authReceiver.NoAuth);

        loop.RegisterRaw<PresenceChallengeCommand>(presenceReceiver.Challenge);
        loop.RegisterRaw<PresenceProofCommand>(presenceReceiver.Proof);

        loop.Register(Authenticated<SpawnPlayerCommand>(spawnPlayerReceiver.Receive));
        loop.Register(Authenticated(DimensionHandler<DimensionMovePlayerReceiver, MovePlayerCommand>()));
        loop.Register(Authenticated(DimensionHandler<DimensionInventoryActionReceiver, InventoryActionCommand>()));
        loop.Register(Authenticated(DimensionHandler<DimensionForgetChunkReceiver, ForgetChunkCommand>()));
        loop.Register(Authenticated(DimensionHandler<DimensionForgetSectionReceiver, ForgetSectionCommand>()));
    }

    private Action<NetSocket, C> Authenticated<C>(Action<NetSocket, C> handler) where C : unmanaged => (ns, cmd) =>
    {
        if (!auth.AuthorizeGameplay(ns))
        {
            if (ns.Connected)
            {
                log.Warn("Socket {0} tried to perform an authenticated action before authenticating", ns.Tag);
                ns.Disconnect();
            }
            return;
        }

        handler(ns, cmd);
    };

    private Action<NetSocket, C> Authenticated<C>(Action<NetSocket> handler) where C : unmanaged =>
        Authenticated<C>((ns, cmd) => handler(ns));

    private Action<NetSocket, C> DimensionHandler<T, C>()
        where T : DimensionReceiver<C> where C : unmanaged => (ns, cmd) =>
    {
        if (ns.SocketWorldPlayer == default ||
            ns.DimensionScope == default ||
            ns.PlayerSpawnPhase != PlayerSpawnPhase.Active)
        {
            log.Warn("Socket {0} tried to perform a dimension action before activation", ns.Tag);
            ns.Disconnect();
            return;
        }

        ns.DimensionScope.Get<T>().Receive(ns, cmd);
    };
}
