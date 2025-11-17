Info("Packaging");

Info("Deleting dist directory");
Delete("dist");

var mods = new List<(string Name, bool IncludeServer)>()
{
    ("Crafthoe.Native", true),
    ("Crafthoe.Native.Backend", true),
    ("Crafthoe.Native.Frontend", false)
};

var modDlls = mods.Select((mod) =>
{
    Dir("src", mod.Name, out var modDir);
    Run($"dotnet build {modDir} -c Release", $"Building mod {mod.Name}", $"Failed to build mod {mod.Name}");
    Dir("bin", mod.Name, "Release", $"{mod.Name}.dll", out var modDll);
    return (Mod: mod, Dll: modDll);
}).ToList();

var runtimes = new List<(string Name, bool CompileClient, bool CompileServer)>()
{
    ("win-x64", true, true),
    ("linux-x64", true, true),
    ("linux-arm64", false, true),
    ("osx-arm64", true, true)
};

var exes = runtimes.SelectMany((runtime) =>
{
    var exes = new List<string>();

    if (runtime.CompileClient)
        Compile("Crafthoe", "Crafthoe", "Crafthoe", true);
    if (runtime.CompileServer)
        Compile("Crafthoe.Server.Cli", "CrafthoeServer", "CrafthoeServer", false);

    return exes;

    string Compile(string projectName, string outputName, string exeName, bool server)
    {
        Dir("src", projectName, out var projectDir);
        Dir("dist", runtime.Name, outputName, out var outDir);

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
                $"-r {runtime.Name}",
                $"-o {outDir}"
            );

            Run(command, $"Publishing {outputName} for {runtime.Name}", $"Failed to publish {outputName} for {runtime.Name}");
        });

        Dir("res", projectName, out var resProjectDir);
        if (Directory.Exists(Absolute(resProjectDir)))
            Copy(resProjectDir, outDir);

        var outMods = server ? modDlls : [.. modDlls.Where(x => x.Mod.IncludeServer)];
        foreach (var (mod, dll) in outMods)
        {
            Dir("res", mod.Name, out var resModDir);
            Dir(outDir, "Mods", mod.Name, out var outModDir);
            Dir(outModDir, $"{mod.Name}.dll", out var outModDll);
            Copy(dll, outModDll);

            if (Directory.Exists(Absolute(resModDir)))
                Copy(resModDir, outModDir);
        }

        Dir(outDir, "Load.txt", out var loadFile);
        File.WriteAllLines(Absolute(loadFile), [.. outMods.Select(x => x.Mod.Name)]);

        Dir(outDir, $"{exeName}{(runtime.Name.StartsWith("win") ? ".exe" : "")}", out var exeFile);
        return exeFile;
    }
}).ToList();

Success($"Packaged");
exes.ForEach(x => Success($"-> {x}"));
