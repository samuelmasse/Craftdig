namespace Craftdig;

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
                using var stream = File.OpenRead(files[fullName]);
                value = pngs.Read(stream);
                images.Add(file, value);
            }

            return value;
        }
    }
}
