namespace Craftdig;

[Module]
public class ModuleMultiplayerServerPinger
{
    private readonly Log log;
    private readonly AppClientOptions clientOptions;
    private readonly ModuleMultiplayerServerCache serverCache;
    private readonly Dictionary<ServerAddress, ServerPingTask> tasks = [];

    public ServerPingResult? this[ServerAddress address] => tasks[address].Result;

    public ModuleMultiplayerServerPinger(
        Log log,
        RootUnload unload,
        AppClientOptions clientOptions,
        ModuleMultiplayerServerCache serverCache)
    {
        this.log = log;
        this.clientOptions = clientOptions;
        this.serverCache = serverCache;
        unload.Add(CancelAll);
    }

    public void PingAll(ReadOnlySpan<ServerEntry> servers)
    {
        CancelAll();

        foreach (var server in servers)
            PingOne(server.Address);
    }

    private void CancelAll()
    {
        foreach (var task in tasks.Values)
        {
            task.Token.Cancel();
            task.Socket?.Disconnect();
            task.Tcp.Dispose();
        }

        foreach (var task in tasks.Values)
        {
            task.Thread?.Join();
            task.Token.Dispose();
        }

        tasks.Clear();
    }

    private void PingOne(ServerAddress address)
    {
        if (tasks.ContainsKey(address))
            return;

        var task = new ServerPingTask(address);

        task.Thread = new Thread(() => RunPingTask(task));
        tasks[address] = task;
        task.Thread.Start();
    }

    private void RunPingTask(ServerPingTask task)
    {
        NetSocket? socket = null;
        Thread? loopThread = null;
        Thread? pushThread = null;
        using var done = new ManualResetEventSlim(false);

        try
        {
            socket = Connect(task);
            task.Socket = socket;
            var result = new PingReply();
            var loop = CreateLoop(result, done);
            loopThread = new Thread(() => { try { loop.Run(socket); } catch { } });
            pushThread = new Thread(() => { try { socket.Push(task.Token.Token); } catch { } });
            loopThread.Start();
            pushThread.Start();

            socket.Send(new ServerStatusCommand()
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DescriptionHash = serverCache.DescriptionHash(task.Address),
                IconHash = serverCache.IconHash(task.Address)
            });
            done.Wait(2000, task.Token.Token);
            task.Result = result.Snapshot();
            serverCache.Save(task.Address, task.Result);
        }
        catch
        {
            task.Result = new() { Success = false };
        }

        socket?.Disconnect();
        loopThread?.Join();
        pushThread?.Join();
        task.Socket = null;
        socket?.ReleaseState();
        task.Tcp.Dispose();
    }

    private NetSocket Connect(ServerPingTask task)
    {
        var tcp = task.Tcp;
        tcp.ConnectAsync(task.Address.Host, task.Address.Port, task.Token.Token).GetAwaiter().GetResult();
        Stream stream = clientOptions.UseRawTcp ? tcp.GetStream() : ClientTls.Connect(log, tcp, task.Address.Host);
        return new(log, tcp, stream);
    }

    private NetLoop CreateLoop(PingReply result, ManualResetEventSlim done)
    {
        var loop = new NetLoop(log);
        loop.Register((NetSocket ns, PongCommand cmd) =>
            result.Ping = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(cmd.Ping.Timestamp));
        loop.Register((NetSocket ns, ServerPopulationCommand cmd) =>
        {
            result.MaxPlayers = cmd.MaxPlayers;
            result.CurrentPlayers = cmd.CurrentPlayers;
        });
        loop.Register((NetSocket ns, ServerDescriptionCommand cmd, ReadOnlySpan<byte> data) =>
            result.Description = Encoding.UTF8.GetString(data));
        loop.Register((NetSocket ns, ServerIconCommand cmd, ReadOnlySpan<byte> data) =>
            result.IconData = data.ToArray());
        loop.Register<ServerStatusDoneCommand>(done.Set);
        return loop;
    }

    private record PingReply
    {
        public TimeSpan? Ping { get; set; }
        public int? MaxPlayers { get; set; }
        public int? CurrentPlayers { get; set; }
        public string? Description { get; set; }
        public byte[]? IconData { get; set; }

        public ServerPingResult Snapshot() => new()
        {
            Success = Ping != null,
            Ping = Ping,
            MaxPlayers = MaxPlayers,
            CurrentPlayers = CurrentPlayers,
            Description = Description,
            IconData = IconData,
        };
    }
}
