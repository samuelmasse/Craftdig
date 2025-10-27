namespace Crafthoe.Server;

[World]
public class WorldServer(
    WorldApplyGlobalSettingsAction applyGlobalSettingsAction,
    WorldLoadOrCreateMetaAction loadOrCreateMetaAction,
    WorldLoadDimensionsAction loadDimensionsAction,
    WorldRegisterHandlersAction registerHandlersAction,
    WorldServerTickTimer tickTimer,
    WorldServerTicks ticks,
    WorldListener listener)
{
    public void Run()
    {
        applyGlobalSettingsAction.Run();
        loadOrCreateMetaAction.Run();
        loadDimensionsAction.Run();
        registerHandlersAction.Run();
        tickTimer.Start();
        new Thread(ticks.Run).Start();
        listener.Start();
    }
}
