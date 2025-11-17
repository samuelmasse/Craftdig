namespace Crafthoe.Server;

[Server]
public class ServerBoot(ModuleScope moduleScope, WorldScope worldScope, ServerScope serverScope)
{
    public void Run(string? root)
    {
        moduleScope.Handler(moduleScope.Get<ModuleEntMutInjector>());
        moduleScope.Scope<ModuleLoaderScope>().Get<ModuleLoader>().Run();

        var config = serverScope.Get<ServerLoadConfigAction>().Run(root);
        serverScope.Add(config);
        worldScope.Add(new WorldPaths(Path.Join(config.RootPath, "World")));
    }
}
