namespace Craftdig.App;

[AppLoader]
public class AppUnloader(AppLog log, AppLogThread logThread)
{
    public void Run()
    {
        log.Info("Unloading app");
        logThread.Stop();
    }
}
