namespace Craftdig;

[App]
public class AppMods(ModEntry[] entries)
{
    public ReadOnlySpan<ModEntry> Entries => entries;
}
