namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerListWorldsAction(
    AppPaths paths,
    ModuleReadWorldMetaAction readWorldMetaAction,
    ModuleReadWorldStateAction readWorldStateAction)
{
    public List<WorldEntry> Run()
    {
        var worlds = new List<WorldEntry>();
        Directory.CreateDirectory(paths.SavePath);

        foreach (var dir in Directory.GetDirectories(paths.SavePath))
        {
            try
            {
                var worldPaths = new WorldPaths(dir);
                var meta = readWorldMetaAction.Read(worldPaths);
                var state = readWorldStateAction.Read(worldPaths);
                worlds.Add(new(dir, worldPaths, meta, state));
            }
            catch { }
        }

        worlds.Sort((a, b) => b.State.LastPlayed.CompareTo(a.State.LastPlayed));
        return worlds;
    }
}
