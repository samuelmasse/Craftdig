namespace Crafthoe.Server;

[Server]
public class ServerLoadConfigAction(ServerScope scope, ServerPaths paths)
{
    public void Run()
    {
        var builder = new ConfigurationBuilder();
        string[] roots = [MachineConfigDir(), UserDir(), ExeDir(), CwdDir(), paths.Root ];

        foreach (var root in roots.Distinct())
        {
            var iniPath = Path.Join(root, "Server.ini");
            if (File.Exists(iniPath))
                Console.WriteLine($"Loaded config {iniPath}");
            builder.AddIniFile(iniPath, true);

            var jsonPath = Path.Join(root, "Server.json");
            if (File.Exists(jsonPath))
                Console.WriteLine($"Loaded config {jsonPath}");
            builder.AddJsonFile(jsonPath, true);
        }

        builder.AddEnvironmentVariables("Crafthoe_");
        scope.Add(builder.Build().Get<ServerConfig>() ?? new());
    }

    private string MachineConfigDir() => OperatingSystem.IsWindows() ?
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Crafthoe") :
        "/etc/crafthoe";
    private string UserDir() => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".crafthoe");
    private string ExeDir() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    private string CwdDir() => Directory.GetCurrentDirectory();
}
