namespace Craftdig.App.Frontend;

[App]
public class AppPaths
{
    public string GamePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Craftdig");
    public string SavePath => Path.Combine(GamePath, "Saves");
}
