namespace Craftdig.World.Backend;

[Module]
public class ModuleReadWorldStateAction
{
    public WorldState Read(WorldPaths paths)
    {
        var lastPlayed = DateTimeOffset.UtcNow;
        var stateFile = Path.Join(paths.Root, "State.toml");

        if (File.Exists(stateFile))
        {
            var text = File.ReadAllText(stateFile);
            var model = Toml.ToModel<WorldStateFile>(text, null, new() { ConvertPropertyName = (s) => s });

            if (model.LastPlayed is long ts)
                lastPlayed = DateTimeOffset.FromUnixTimeMilliseconds(ts);
        }

        return new(lastPlayed);
    }
}
