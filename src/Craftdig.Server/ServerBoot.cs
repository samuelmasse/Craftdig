namespace Craftdig.Server;

[Server]
public class ServerBoot(AppLog log, ModuleScope moduleScope, WorldScope worldScope, ServerScope serverScope)
{
    public void Run(string[] args)
    {
        log.Raw(@"");
        log.Raw(@"  /$$$$$$                      /$$$$$$   /$$           /$$ /$$          ");
        log.Raw(@" /$$__  $$                    /$$__  $$ | $$          | $$|__/          ");
        log.Raw(@"| $$  \__/  /$$$$$$  /$$$$$$ | $$  \__//$$$$$$    /$$$$$$$ /$$  /$$$$$$ ");
        log.Raw(@"| $$       /$$__  $$|____  $$| $$$$   |_  $$_/   /$$__  $$| $$ /$$__  $$");
        log.Raw(@"| $$      | $$  \__/ /$$$$$$$| $$_/     | $$    | $$  | $$| $$| $$  \ $$");
        log.Raw(@"| $$    $$| $$      /$$__  $$| $$       | $$ /$$| $$  | $$| $$| $$  | $$");
        log.Raw(@"|  $$$$$$/| $$     |  $$$$$$$| $$       |  $$$$/|  $$$$$$$| $$|  $$$$$$$");
        log.Raw(@" \______/ |__/      \_______/|__/        \___/   \_______/|__/ \____  $$");
        log.Raw(@"                                                               /$$  \ $$");
        log.Raw(@"                                                              |  $$$$$$/");
        log.Raw(@"                                                               \______/");
        log.Raw(@"");

        var config = serverScope.Get<ServerLoadConfigAction>().Run(args);
        log.Level = config.LogLevel;

        log.Info("Booting up");

        serverScope.Add(config);
        worldScope.Add(new WorldPaths(Path.Join(config.RootPath, "World")));

        moduleScope.Handler(moduleScope.Get<ModuleEntMutInjector>());
        moduleScope.Scope<ModuleLoaderScope>().Get<ModuleLoader>().Run();
    }
}
