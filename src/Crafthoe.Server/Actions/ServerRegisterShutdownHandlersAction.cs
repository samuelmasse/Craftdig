namespace Crafthoe.Server;

[Server]
public class ServerRegisterShutdownHandlersAction(ServerShutdownAction shutdownAction)
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
                    Console.WriteLine("Received SIGINT");
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
                    Console.WriteLine("Received SIGTERM");
                    shutdownAction.Run();
                }
            }
        };
    }
}
