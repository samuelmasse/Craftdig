namespace Crafthoe.Module.Frontend;

[AppLoader]
public class AppFrontendLoader(AppFiles files, AppMods mods)
{
    public void Run()
    {
        foreach (var entry in mods.Entries)
        {
            if (entry.Root != null)
                files.AddRoot(entry.Root);
        }
    }
}
