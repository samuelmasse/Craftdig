namespace Crafthoe.Frontend;

[Module]
public class ModuleCreateWorldAction(AppPaths paths)
{
    public WorldPaths Run(WorldMeta args)
    {
        string sanitizedName = SanitizeFolderName(args.Name);
        string unusedName = BumpUsedName(sanitizedName);
        string folder = Path.Join(paths.SavePath, unusedName);

        var metadata = Toml.FromModel(new WorldMetadataFile()
        {
            Name = args.Name,
            Seed = args.Seed,
            GameMode = args.GameMode.ModuleName(),
            Difficulty = args.Difficulty.ModuleName()
        }, new() { ConvertPropertyName = (s) => s });

        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Join(folder, "Metadata.toml"), metadata);

        return new(folder);
    }

    private string SanitizeFolderName(string name)
    {
        HashSet<char> invalid = [.. Path.GetInvalidFileNameChars(), '.'];

        var sb = new StringBuilder();

        foreach (char c in name.Trim())
        {
            if (!invalid.Contains(c))
                sb.Append(c);
        }

        return sb.ToString();
    }

    private string BumpUsedName(string name)
    {
        if (!Directory.Exists(Path.Join(paths.SavePath, name)))
            return name;

        int index = 1;
        while (Directory.Exists(Path.Join(paths.SavePath, name + $" ({index})")))
            index++;

        return name + $" ({index})";
    }
}
