namespace Craftdig.Menus.Multiplayer;

[Module]
public class ModuleMultiplayerServerPinger(Log log, AppClientOptions clientOptions, ModuleMultiplayerServerCache serverCache)
{
    private readonly Dictionary<ServerAddress, ServerPingTask> tasks = [];

    public ServerPingResult? this[ServerAddress address] => tasks[address].Result;

    public void PingAll(ReadOnlySpan<ServerEntry> servers)
    {
        CancelAll();

        tasks.Clear();

        foreach (var server in servers)
            PingOne(server.Address);
    }

    private void CancelAll()
    {
        foreach (var task in tasks.Values)
        {
            task.Token?.Cancel();
            task.Socket?.Disconnect();
        }

        tasks.Clear();
    }

    private void PingOne(ServerAddress address)
    {
        if (tasks.ContainsKey(address))
            return;

        var task = new ServerPingTask { Address = address };

        task.Thread = new Thread(() => RunPingTask(task));
        tasks[address] = task;
        task.Thread.Start();
    }

    private void RunPingTask(ServerPingTask task)
    {
        TcpClient? tcp = null;
        NetSocket? socket = null;

        try
        {
            tcp = new TcpClient() { NoDelay = true };
            tcp.Connect(task.Address.Host, task.Address.Port);

            socket = new(
                log,
                tcp,
                clientOptions.UseRawTcp
                    ? tcp.GetStream()
                    : ClientTls.Connect(log, tcp, task.Address.Host));

            var loop = new NetLoop(log);
            var done = new ManualResetEventSlim(false);
            TimeSpan? ping = null;
            int? maxPlayers = null;
            int? currentPlayers = null;
            string? description = null;
            byte[]? iconData = null;

            loop.Register((NetSocket ns, PongCommand cmd) =>
            {
                ping = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(cmd.Ping.Timestamp);
            });

            loop.Register((NetSocket ns, ServerPopulationCommand cmd) =>
            {
                maxPlayers = cmd.MaxPlayers;
                currentPlayers = cmd.CurrentPlayers;
            });

            loop.Register((NetSocket ns, ServerDescriptionCommand cmd, ReadOnlySpan<byte> data) =>
            {
                description = Encoding.UTF8.GetString(data);
            });

            loop.Register((NetSocket ns, ServerIconCommand cmd, ReadOnlySpan<byte> data) =>
            {
                iconData = data.ToArray();
            });

            loop.Register<ServerStatusDoneCommand>(done.Set);

            var loopThread = new Thread(() => { try { loop.Run(socket); } catch { } });
            var pushThread = new Thread(() => { try { socket.Push(task.Token.Token); } catch { } });
            loopThread.Start();
            pushThread.Start();

            socket.Send(new ServerStatusCommand()
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DescriptionHash = serverCache.DescriptionHash(task.Address),
                IconHash = serverCache.IconHash(task.Address)
            });
            done.Wait(2000, task.Token.Token);
            task.Result = new()
            {
                Success = ping != null,
                Ping = ping,
                MaxPlayers = maxPlayers,
                CurrentPlayers = currentPlayers,
                Description = description,
                IconData = iconData
            };

            serverCache.Save(task.Address, task.Result);

            socket.Disconnect();
            loopThread.Join();
            pushThread.Join();
        }
        catch
        {
            task.Result = new() { Success = false };
            socket?.Disconnect();
            tcp?.Dispose();
        }
    }
}
