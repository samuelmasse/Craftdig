namespace Crafthoe.Server;

[Server]
public class ServerRegisterShutdownHandlersAction(AppLog log, ServerShutdownAction shutdownAction)
{
    private bool shuttingDown = false;

    public void Run()
    {
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            ShutDown();
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) => ShutDown();
        AppDomain.CurrentDomain.UnhandledException += (_, _) => ShutDown();
    }

    private void ShutDown()
    {
        lock (this)
        {
            if (!shuttingDown)
            {
                shuttingDown = true;
                log.Info("Received SIGTERM");
                shutdownAction.Run();
            }
        }
    }
}
