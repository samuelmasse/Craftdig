namespace Crafthoe.Server;

[Server]
public class Server(
    ServerApplyGlobalSettingsAction applyGlobalSettingsAction,
    ServerLoadOrCreateMetaAction loadOrCreateMetaAction,
    ServerLoadDimensionsAction loadDimensionsAction,
    ServerRegisterHandlersAction registerHandlersAction,
    ServerTickTimer tickTimer,
    ServerTicks ticks,
    ServerListener listener,
    ServerListenerTls listenerTls)
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
        ticks.Join();
        tickTimer.Stop();

        listener.Join();
        listenerTls.Join();
    }
}
