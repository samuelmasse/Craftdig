namespace Crafthoe.App;

[App]
public class AppLogThread(AppLogStream logStream, AppLogConsole logConsole)
{
    private readonly SemaphoreSlim streamSemaphore = new(0);
    private readonly SemaphoreSlim consoleSemaphore = new(0);
    private readonly SemaphoreSlim streamFinishSemaphore = new(0);
    private readonly SemaphoreSlim consoleFinishSemaphore = new(0);
    private Timer? timer;
    private Thread? streamThread;
    private Thread? consoleThread;
    private bool finish;
    private bool stop;

    public void Start()
    {
        timer = new Timer((e) =>
        {
            streamSemaphore.Release();
            consoleSemaphore.Release();
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(3));

        streamThread = new Thread(() =>
        {
            while (!stop)
            {
                streamSemaphore.Wait();

                if (finish)
                {
                    logStream.Collect(0);
                    streamFinishSemaphore.Release();
                }
                else logStream.Collect();
            }
        });
        streamThread.Start();

        consoleThread = new Thread(() =>
        {
            while (!stop)
            {
                consoleSemaphore.Wait();
                logConsole.Print();
                if (finish)
                    consoleFinishSemaphore.Release();
            }
        });
        consoleThread.Start();
    }

    public void Stop()
    {
        finish = true;
        streamFinishSemaphore.Wait();
        streamFinishSemaphore.Wait();
        consoleFinishSemaphore.Wait();
        consoleFinishSemaphore.Wait();

        stop = true;
        streamSemaphore.Release(ushort.MaxValue);
        consoleSemaphore.Release(ushort.MaxValue);

        timer?.Dispose();
        streamThread?.Join();
        consoleThread?.Join();
    }
}
