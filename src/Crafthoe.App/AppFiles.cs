namespace Crafthoe.App;

[App]
public class AppFiles
{
    private readonly string baseDirectoy;
    private readonly string projectDirectory;

    public AppFiles()
    {
        baseDirectoy = AppDomain.CurrentDomain.BaseDirectory;
        projectDirectory = Path.Join(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(baseDirectoy))))!, "res");
    }

    public string this[string path]
    {
        get
        {
            string projectPath = Path.Join(projectDirectory, path);
            if (File.Exists(projectPath))
                return projectPath;

            string basePath = Path.Join(baseDirectoy, path);
            if (File.Exists(basePath))
                return basePath;

            throw new Exception($"File not found {path}");
        }
    }
}
