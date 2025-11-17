namespace Crafthoe.Server;

[Server]
public class ServerLoadConfigAction
{
    public ServerConfig Run(string[] args)
    {
        var roots = new List<string>() { MachineConfigDir(), UserDir(), ExeDir(), CwdDir() }
            .Select(Path.GetFullPath)
            .Select(Path.TrimEndingDirectorySeparator)
            .ToList();

        var config = ReadConfig(roots, args, null);
        var dynamicRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(config.RootPath ?? CwdDir()));
        roots.Add(dynamicRoot);
        return ReadConfig(roots, args, dynamicRoot);
    }

    private ServerConfig ReadConfig(List<string> roots, string[] args, string? rootPath)
    {
        var builder = new ConfigurationBuilder();

        foreach (var root in roots)
        {
            builder.AddIniFile(Path.Join(root, "Server.ini"), true);
            builder.AddJsonFile(Path.Join(root, "Server.json"), true);
        }

        builder.AddEnvironmentVariables("Crafthoe_");
        builder.AddCommandLine(args);

        if (rootPath != null)
        {
            var seen = new HashSet<string>();

            for (int i = roots.Count - 1; i >= 0; i--)
            {
                var root = roots[i];
                if (seen.Contains(root))
                    continue;

                var indent = new string(' ', seen.Count * 2);
                var iniPath = Path.Join(root, "Server.ini");
                if (File.Exists(iniPath))
                    Console.WriteLine($"{indent}Config {iniPath}");
                var jsonPath = Path.Join(root, "Server.json");
                if (File.Exists(jsonPath))
                    Console.WriteLine($"{indent}Config {jsonPath}");

                seen.Add(root);
            }

            builder.AddInMemoryCollection(new Dictionary<string, string?> { { "RootPath", rootPath } });
        }

        return builder.Build().Get<ServerConfig>() ?? new();
    }

    private string MachineConfigDir() => OperatingSystem.IsWindows() ?
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Crafthoe") :
        "/etc/crafthoe";
    private string UserDir() => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".crafthoe");
    private string ExeDir() => AppContext.BaseDirectory;
    private string CwdDir() => Directory.GetCurrentDirectory();
}
