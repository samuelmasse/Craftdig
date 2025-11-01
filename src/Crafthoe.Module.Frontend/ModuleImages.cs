namespace Crafthoe.Module.Frontend;

[Module]
public class ModuleImages(RootPngs pngs, AppFiles files)
{
    private readonly Dictionary<string, ImageData> images = [];

    public ImageData this[string file]
    {
        get
        {
            if (!images.TryGetValue(file, out var value))
            {
                string fullName = Path.Combine("Textures", file) + ".png";
                value = pngs[files[fullName]];
                images.Add(file, value);
            }

            return value;
        }
    }
}
