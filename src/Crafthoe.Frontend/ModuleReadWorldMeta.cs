namespace Crafthoe.Frontend;

[Module]
public class ModuleReadWorldMeta(ModuleEnts ents)
{
    public WorldMeta Read(WorldPaths paths)
    {
        var metadataFile = Path.Join(paths.Root, "Metadata.toml");

        var text = File.ReadAllText(metadataFile);
        var model = Toml.ToModel<WorldMetadataFile>(text, null, new() { ConvertPropertyName = (s) => s });

        return new(model.Name!, model.Seed!.Value, ents[model.GameMode!], ents[model.Difficulty!]);
    }
}
