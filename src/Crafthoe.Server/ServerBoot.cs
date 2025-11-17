namespace Crafthoe.Server;

[Server]
public class ServerBoot(ModuleScope moduleScope, WorldScope worldScope, ServerScope serverScope)
{
    public void Run(string? root)
    {
        moduleScope.Handler(moduleScope.Get<ModuleEntMutInjector>());
        moduleScope.Scope<ModuleLoaderScope>().Get<ModuleLoader>().Run();
        serverScope.Add(serverScope.Get<ServerLoadConfigAction>().Run(root));
        worldScope.Add(new WorldPaths(Path.Join(root, "World")));
    }
}
