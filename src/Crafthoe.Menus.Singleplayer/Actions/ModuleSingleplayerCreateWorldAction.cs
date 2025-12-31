namespace Craftdig.Menus.Singleplayer;

[Module]
public class ModuleSingleplayerCreateWorldAction(AppPaths paths, ModuleWriteWorldMetaAction writeWorldMetaAction)
{
    public WorldPaths Run(WorldMeta args)
    {
        string sanitizedName = SanitizeFolderName(args.Name);
        string unusedName = BumpUsedName(sanitizedName);
        string folder = Path.Join(paths.SavePath, unusedName);

        var worldPaths = new WorldPaths(folder);
        writeWorldMetaAction.Write(args, worldPaths);
        return worldPaths;
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
