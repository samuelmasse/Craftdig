namespace Crafthoe.Server;

[Server]
public class ServerBoot(ModuleScope moduleScope, WorldScope worldScope, ServerScope serverScope)
{
    public void Run(string root)
    {
        moduleScope.Handler(moduleScope.Get<ModuleEntMutInjector>());
        moduleScope.Scope<ModuleLoaderScope>().Get<ModuleLoader>().Run();

        var serverDir = Path.Join(root, "Data");
        var worldDir = Path.Join(serverDir, "World");

        worldScope.Add(new WorldPaths(worldDir));
        serverScope.Add(new ServerPaths(serverDir));
    }
}
