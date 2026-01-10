namespace Craftdig.World.Backend;

[Module]
public class ModuleWriteWorldMetaAction
{
    public void Write(WorldMeta args, WorldPaths paths)
    {
        var metadata = Toml.FromModel(new WorldMetadataFile()
        {
            Name = args.Name,
            Seed = args.Seed,
            GameMode = args.GameMode.ModuleName(),
            Difficulty = args.Difficulty.ModuleName()
        }, new() { ConvertPropertyName = (s) => s });

        Directory.CreateDirectory(paths.Root);
        File.WriteAllText(Path.Join(paths.Root, "Metadata.toml"), metadata);
    }
}
