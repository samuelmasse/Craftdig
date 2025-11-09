namespace Crafthoe.Server;

[Server]
public class ServerLoadOrCreateMetaAction(
    ModuleWriteWorldMetaAction writeWorldMetaAction,
    ModuleReadWorldMetaAction readWorldMetaAction,
    WorldScope scope,
    ServerDefaults defaults,
    WorldPaths paths)
{
    public void Run()
    {
        if (!Directory.Exists(paths.Root))
        {
            int seed = defaults.Seed ?? new Random().Next();
            writeWorldMetaAction.Write(new(defaults.Name, seed, defaults.GameMode, defaults.Difficulty), paths);
        }

        scope.Add(readWorldMetaAction.Read(paths));
    }
}
