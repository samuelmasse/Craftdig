namespace Craftdig;

[ModuleLoader]
public class ModuleLoader(Log log, AppMods mods, ModuleEnts ents, ModuleLoaderScope scope)
{
    public void Run()
    {
        foreach (var entry in mods.Entries)
            ((ModLoader)scope.Get(entry.Loader)).Load();

        log.Info("Loaded {0} ents", ents.Span.Length);
    }
}
