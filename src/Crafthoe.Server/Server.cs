namespace Crafthoe.Server;

[Server]
public class Server(
    ServerApplyGlobalSettingsAction applyGlobalSettingsAction,
    ServerLoadOrCreateMetaAction loadOrCreateMetaAction,
    ServerLoadDimensionsAction loadDimensionsAction,
    ServerRegisterHandlersAction registerHandlersAction,
    ServerRegisterShutdownHandlersAction registerShutdownHandlersAction,
    ServerTickTimer tickTimer,
    ServerTicks ticks,
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
        unloadDimensionsAction.Run();
    }
}
