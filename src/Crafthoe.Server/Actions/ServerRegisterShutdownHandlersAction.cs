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

            lock (this)
            {
                if (!shuttingDown)
                {
                    shuttingDown = true;
                    log.Info("Received SIGINT");
                    shutdownAction.Run();
                }
            }
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
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
        };
    }
}
