Info("Packaging");

Info("Deleting dist directory");
Delete("dist");

var mods = new List<string>()
{
    "Crafthoe.Native",
    "Crafthoe.Native.Backend",
    "Crafthoe.Native.Frontend"
};

var modDlls = mods.Select((mod) =>
{
    Dir("src", mod, out var modDir);
    Run($"dotnet build {modDir} -c Release", $"Building mod {mod}", $"Failed to build mod {mod}");
    Dir("bin", mod, "Release", $"{mod}.dll", out var modDll);
    return modDll;
}).ToList();

var runtimes = new List<string>()
{
    "win-x64",
    "linux-x64",
    "osx-arm64"
};

var exes = runtimes.SelectMany<string, string>((runtime) =>
{
    var clientExe = Compile("Crafthoe", "Crafthoe", "Crafthoe", true);
    var serverExe = Compile("Crafthoe.Server.Cli", "CrafthoeServer", "CrafthoeServer", false);

    string Compile(string projectName, string outputName, string exeName, bool includeRes)
    {
        Dir("src", projectName, out var projectDir);
        Dir("dist", runtime, outputName, out var outDir);

        Section(() =>
        {
            var command = string.Join(' ',
                "dotnet",
                "publish",
                projectDir,
                "-c Release",
                "--self-contained",
                "-p:PublishSingleFile=true",
                "-p:IncludeNativeLibrariesForSelfExtract=true",
                "-p:DebugType=None",
                $"-r {runtime}",
                $"-o {outDir}"
            );

            Run(command, $"Publishing for {runtime}", $"Failed to publish for {runtime}");
        });

        Dir("res", projectName, out var resProjectDir);
        if (Directory.Exists(Absolute(resProjectDir)))
            Copy(resProjectDir, outDir);

        var outMods = includeRes ? mods : [.. mods.SkipLast(1)];
        for (int i = 0; i < outMods.Count; i++)
        {
            var mod = mods[i];
            var modDll = modDlls[i];

            Dir("res", mod, out var resModDir);
            Dir(outDir, "Mods", mod, out var outModDir);
            Dir(outModDir, $"{mod}.dll", out var outModDll);
            Copy(modDll, outModDll);

            if (Directory.Exists(Absolute(resModDir)))
                Copy(resModDir, outModDir);
        }

        Dir(outDir, "Load.txt", out var loadFile);
        File.WriteAllLines(Absolute(loadFile), [.. outMods]);

        Dir(outDir, $"{exeName}{(runtime.StartsWith("win") ? ".exe" : "")}", out var exeFile);
        return exeFile;
    }

    return [clientExe, serverExe];
}).ToList();

Success($"Packaged");
exes.ForEach(x => Success($"-> {x}"));
