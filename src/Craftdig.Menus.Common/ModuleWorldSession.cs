namespace Craftdig;

[Module]
public class ModuleWorldSession
{
    private Action? unload;

    public void Load(Action unloadWorld) => unload = unloadWorld;

    public void Unload()
    {
        unload?.Invoke();
        unload = null;
    }
}
