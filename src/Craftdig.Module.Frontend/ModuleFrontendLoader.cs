namespace Craftdig.Module.Frontend;

[ModuleLoader]
public class ModuleFrontendLoader(AppFiles files, AppMods mods, ModuleFaceLoader faceLoader)
{
    public void Run()
    {
        foreach (var entry in mods.Entries)
        {
            if (entry.Root != null)
                files.AddRoot(entry.Root);
        }

        faceLoader.Run();
    }
}
