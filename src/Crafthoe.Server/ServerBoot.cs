namespace Crafthoe.Server;

[Server]
public class ServerBoot(ModuleScope moduleScope, WorldScope worldScope, ServerScope serverScope)
{
    public void Run(string[] args)
    {
        var config = serverScope.Get<ServerLoadConfigAction>().Run(args);
        serverScope.Add(config);
        worldScope.Add(new WorldPaths(Path.Join(config.RootPath, "World")));

        moduleScope.Handler(moduleScope.Get<ModuleEntMutInjector>());
        moduleScope.Scope<ModuleLoaderScope>().Get<ModuleLoader>().Run();
    }
}
