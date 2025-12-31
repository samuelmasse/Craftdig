namespace Craftdig.App;

[App]
public class AppMods(ModEntry[] entries)
{
    public ReadOnlySpan<ModEntry> Entries => entries;
}
