namespace Craftdig.Server;

[Server]
public class ServerLoadOrCreateMetaAction(
    ModuleEnts ents,
    ModuleWriteWorldMetaAction writeWorldMetaAction,
    ModuleReadWorldMetaAction readWorldMetaAction,
    WorldScope scope,
    WorldPaths paths)
{
    public void Run()
    {
        if (!Directory.Exists(paths.Root))
        {
            int seed = new Random().Next();
            var gameModes = ents.Set.Where(x => x.IsGameMode()).OrderBy(x => x.Order()).ToArray();
            var difficulties = ents.Set.Where(x => x.IsDifficulty()).OrderBy(x => x.Order()).ToArray();
            writeWorldMetaAction.Write(new("Server", seed, gameModes[0], difficulties[0]), paths);
        }

        scope.Add(readWorldMetaAction.Read(paths));
    }
}
