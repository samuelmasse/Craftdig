namespace Crafthoe.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerConnectAction
{
    private string? host;
    private int port;
    private Thread? thread;
    private Socket? socket;
    private Exception? exception;

    public string? Host => host;
    public int Port => port;
    public bool Connecting => thread != null;
    public Socket? Socket => socket;
    public Exception? Exception => exception;

    public void Start(string host, int port)
    {
        while (thread != null)
            Thread.Sleep(10);

        this.host = host;
        this.port = port;

        socket = null;
        exception = null;

        thread = new Thread(() =>
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                socket.Connect(host, port);
            }
            catch (Exception e)
            {
                try { socket?.Dispose(); } catch { }
                socket = null;
                exception = e;
            }
            finally
            {
                thread = null;
            }
        });

        thread.Start();
    }

    public void Cancel()
    {
        if (thread == null)
            return;

        try { socket?.Dispose(); } catch { }
    }
}
