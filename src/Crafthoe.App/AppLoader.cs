namespace Craftdig.App;

[AppLoader]
public class AppLoader(AppLogThread logThread)
{
    public void Run()
    {
        logThread.Start();
    }
}
