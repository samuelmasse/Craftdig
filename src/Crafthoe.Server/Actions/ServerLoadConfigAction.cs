namespace Crafthoe.Server;

using Microsoft.Extensions.Configuration;

[Server]
public class ServerLoadConfigAction(ServerScope scope, ServerPaths paths)
{
    public void Run()
    {
        var builder = new ConfigurationBuilder()
            .AddIniFile(Path.Join(paths.Root, "Server.ini"), true)
            .AddJsonFile(Path.Join(paths.Root, "Server.json"), true)
            .AddEnvironmentVariables("Crafthoe");

        var config = builder.Build().Get<ServerConfig>() ?? new();
        scope.Add(config);
    }
}
