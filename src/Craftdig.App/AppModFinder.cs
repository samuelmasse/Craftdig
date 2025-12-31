namespace Craftdig.App;

[AppLoader]
public class AppModFinder
{
    public ModEntry[] Find()
    {
        var root = AppDomain.CurrentDomain.BaseDirectory;
        var modDir = Path.Join(root, "Mods");
        var loadTxt = File.ReadAllLines(Path.Join(root, "Load.txt"));
        var load = loadTxt.Select(x => x.Trim()).Where(x => x.Length > 0);
        var entries = new List<ModEntry>();

        foreach (var name in load)
        {
            var dir = Path.Join(modDir, name);
            var files = Directory.GetFiles(dir);
            var dlls = files.Where(x => x.EndsWith(".dll")).ToArray();
            if (dlls.Length == 0)
                continue;
            if (dlls.Length > 1)
                throw new Exception("Mod has too many dlls");

            var asm = Assembly.LoadFrom(dlls[0]);
            var loader = asm.GetTypes().FirstOrDefault(x => x.IsAssignableTo(typeof(ModLoader))) ??
                throw new Exception("Mod doesn't define a ModLoader");

            entries.Add(new(loader, dir));
        }

        return [.. entries];
    }
}
