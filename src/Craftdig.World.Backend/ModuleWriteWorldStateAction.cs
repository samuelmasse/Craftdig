namespace Craftdig;

[Module]
public class ModuleWriteWorldStateAction
{
    public void Write(WorldState args, WorldPaths paths)
    {
        var state = Toml.FromModel(new WorldStateFile()
        {
            LastPlayed = args.LastPlayed.ToUnixTimeMilliseconds()
        }, new() { ConvertPropertyName = (s) => s });

        Directory.CreateDirectory(paths.Root);
        File.WriteAllText(Path.Join(paths.Root, "State.toml"), state);
    }
}
