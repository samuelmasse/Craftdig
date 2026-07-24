namespace Craftdig.Server;

[Server]
public class Server(
    ServerApplyGlobalSettingsAction applyGlobalSettingsAction,
    ServerLoadOrCreateMetaAction loadOrCreateMetaAction,
    ServerLoadDimensionsAction loadDimensionsAction,
    ServerRegisterHandlersAction registerHandlersAction,
    ServerRegisterShutdownHandlersAction registerShutdownHandlersAction,
    ServerTickTimer tickTimer,
    ServerTicks ticks,
    ServerPresenceLoop presenceLoop,
    ServerListener listener,
    ServerListenerTls listenerTls,
    ServerDrainSocketsAction drainSocketsAction,
    ServerUnloadDimensionsAction unloadDimensionsAction)
{
    public void Run()
    {
        applyGlobalSettingsAction.Run();
        loadOrCreateMetaAction.Run();
        loadDimensionsAction.Run();
        registerHandlersAction.Run();

        presenceLoop.Start();
        listener.Start();
        listenerTls.Start();
        tickTimer.Start();
        ticks.Start();
        registerShutdownHandlersAction.Run();

        ticks.Join();
        tickTimer.Stop();
        listener.Join();
        listenerTls.Join();

        drainSocketsAction.Run();
        presenceLoop.Stop();
        presenceLoop.Join();
        unloadDimensionsAction.Run();
    }
}
