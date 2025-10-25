namespace Crafthoe.Server;

[World]
public class WorldServer(
    WorldApplyGlobalSettingsAction applyGlobalSettingsAction,
    WorldLoadOrCreateMetaAction loadOrCreateMetaAction,
    WorldLoadDimensionsAction loadDimensionsAction,
    WorldRegisterHandlersAction registerHandlersAction,
    WorldTicks ticks,
    WorldListener listener)
{
    public void Run()
    {
        applyGlobalSettingsAction.Run();
        loadOrCreateMetaAction.Run();
        loadDimensionsAction.Run();
        registerHandlersAction.Run();
        new Thread(ticks.Run).Start();
        listener.Start();
    }
}
