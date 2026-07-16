var repositoryRoot = FindRepositoryRoot(AppContext.BaseDirectory)
    ?? FindRepositoryRoot(Environment.CurrentDirectory)
    ?? throw new InvalidOperationException("Could not locate Craftdig repository root.");

Console.WriteLine("Packaging");

var distDirectory = Path.Combine(repositoryRoot, "dist");
Console.WriteLine($"Deleting {distDirectory}");
if (Directory.Exists(distDirectory))
    Directory.Delete(distDirectory, recursive: true);

var mods = new List<(string Name, bool IncludeInServer)>()
{
    ("Craftdig.Native", true),
    ("Craftdig.Native.Backend", true),
    ("Craftdig.Native.Frontend", false)
};

var builtMods = new List<((string Name, bool IncludeInServer) Mod, string Dll)>(mods.Count);
foreach (var mod in mods)
{
    var modProjectDirectory = Path.Combine(repositoryRoot, "src", mod.Name);
    Console.WriteLine($"Building mod {mod.Name}");
    RunProcess(
        new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = repositoryRoot,
            ArgumentList = { "build", modProjectDirectory, "-c", "Release" }
        },
        $"Failed to build mod {mod.Name}");

    builtMods.Add((mod, Path.Combine(repositoryRoot, "bin", mod.Name, "Release", $"{mod.Name}.dll")));
}

var runtimes = new List<(string Name, bool CompileClient, bool CompileServer)>()
{
    ("win-x64", true, true),
    ("linux-x64", true, true),
    ("linux-arm64", false, true),
    ("osx-arm64", true, true)
};

var exes = new List<string>();
foreach (var (name, compileClient, compileServer) in runtimes)
{
    if (compileClient)
        exes.Add(Publish("Craftdig", "Craftdig", "Craftdig", name, includeClientOnlyMods: true));
    if (compileServer)
        exes.Add(Publish("Craftdig.Server.Cli", "CraftdigServer", "CraftdigServer", name, includeClientOnlyMods: false));
}

Console.WriteLine("Packaged");
foreach (var exe in exes)
    Console.WriteLine($"-> {exe}");

string Publish(
    string projectName,
    string outputName,
    string exeName,
    string runtime,
    bool includeClientOnlyMods)
{
    var projectDirectory = Path.Combine(repositoryRoot, "src", projectName);
    var outputDirectory = Path.Combine(repositoryRoot, "dist", runtime, outputName);

    Console.WriteLine($"Publishing {outputName} for {runtime}");
    RunProcess(
        new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = repositoryRoot,
            ArgumentList =
            {
                "publish",
                projectDirectory,
                "-c",
                "Release",
                "--self-contained",
                "-p:PublishSingleFile=true",
                "-p:PublishDocumentationFiles=false",
                "-p:IncludeNativeLibrariesForSelfExtract=true",
                "-p:DebugType=embedded",
                "-r",
                runtime,
                "-o",
                outputDirectory
            }
        },
        $"Failed to publish {outputName} for {runtime}");

    var projectResourceDirectory = Path.Combine(repositoryRoot, "res", projectName);
    if (Directory.Exists(projectResourceDirectory))
        CopyDirectory(projectResourceDirectory, outputDirectory);

    foreach (var (mod, dll) in builtMods)
    {
        if (!includeClientOnlyMods && !mod.IncludeInServer)
            continue;

        var modOutputDirectory = Path.Combine(outputDirectory, "Mods", mod.Name);
        Directory.CreateDirectory(modOutputDirectory);
        File.Copy(dll, Path.Combine(modOutputDirectory, $"{mod.Name}.dll"), overwrite: true);

        var modResourceDirectory = Path.Combine(repositoryRoot, "res", mod.Name);
        if (Directory.Exists(modResourceDirectory))
            CopyDirectory(modResourceDirectory, modOutputDirectory);
    }

    var loadedMods = includeClientOnlyMods
        ? builtMods.Select(mod => mod.Mod.Name)
        : builtMods.Where(mod => mod.Mod.IncludeInServer).Select(mod => mod.Mod.Name);
    File.WriteAllLines(Path.Combine(outputDirectory, "Load.txt"), loadedMods);

    var extension = runtime.StartsWith("win") ? ".exe" : "";
    return Path.Combine(outputDirectory, $"{exeName}{extension}");
}

string? FindRepositoryRoot(string start)
{
    var current = new DirectoryInfo(Path.GetFullPath(start));
    while (current is not null)
    {
        if (File.Exists(Path.Combine(current.FullName, "Craftdig.slnx")))
            return current.FullName;

        current = current.Parent;
    }

    return null;
}

void RunProcess(ProcessStartInfo startInfo, string failureMessage)
{
    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException($"Failed to start {startInfo.FileName}.");
    process.WaitForExit();

    if (process.ExitCode != 0)
        throw new InvalidOperationException($"{failureMessage}. Exit code: {process.ExitCode}.");
}

void CopyDirectory(string sourceDirectory, string destinationDirectory)
{
    Directory.CreateDirectory(destinationDirectory);

    foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        File.Copy(file, Path.Combine(destinationDirectory, Path.GetFileName(file)), overwrite: true);

    foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        CopyDirectory(directory, Path.Combine(destinationDirectory, Path.GetFileName(directory)));
}
