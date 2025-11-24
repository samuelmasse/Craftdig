namespace Crafthoe.Server;

[Server]
public class ServerBoot(AppLog log, ModuleScope moduleScope, WorldScope worldScope, ServerScope serverScope)
{
    public void Run(string[] args)
    {
        log.Raw(
            """

              /$$$$$$                      /$$$$$$   /$$     /$$
             /$$__  $$                    /$$__  $$ | $$    | $$
            | $$  \__/  /$$$$$$  /$$$$$$ | $$  \__//$$$$$$  | $$$$$$$   /$$$$$$   /$$$$$$
            | $$       /$$__  $$|____  $$| $$$$   |_  $$_/  | $$__  $$ /$$__  $$ /$$__  $$
            | $$      | $$  \__/ /$$$$$$$| $$_/     | $$    | $$  \ $$| $$  \ $$| $$$$$$$$
            | $$    $$| $$      /$$__  $$| $$       | $$ /$$| $$  | $$| $$  | $$| $$_____/
            |  $$$$$$/| $$     |  $$$$$$$| $$       |  $$$$/| $$  | $$|  $$$$$$/|  $$$$$$$
             \______/ |__/      \_______/|__/        \___/  |__/  |__/ \______/  \_______/

            """);

        log.Info("Booting up");

        var config = serverScope.Get<ServerLoadConfigAction>().Run(args);
        serverScope.Add(config);
        worldScope.Add(new WorldPaths(Path.Join(config.RootPath, "World")));

        moduleScope.Handler(moduleScope.Get<ModuleEntMutInjector>());
        moduleScope.Scope<ModuleLoaderScope>().Get<ModuleLoader>().Run();
    }
}
