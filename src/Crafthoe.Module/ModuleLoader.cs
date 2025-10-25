namespace Crafthoe.Module;

[ModuleLoader]
public class ModuleLoader(AppMods mods, ModuleEnts ents, ModuleLoaderScope scope)
{
    public void Run()
    {
        foreach (var entry in mods.Entries)
            ((ModLoader)scope.Get(entry.Loader)).Load();

        Console.WriteLine($"Loaded {ents.Span.Length} entities");
    }
}
