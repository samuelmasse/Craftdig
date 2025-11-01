namespace Crafthoe.App.Frontend;

[App]
public class AppPaths
{
    public string GamePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Crafthoe");
    public string SavePath => Path.Combine(GamePath, "Saves");
}
