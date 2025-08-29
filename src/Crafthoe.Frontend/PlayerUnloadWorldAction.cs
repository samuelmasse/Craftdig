namespace Crafthoe.Frontend;

[Player]
public class PlayerUnloadWorldAction(WorldScope worldScope)
{
    public void Run()
    {
        worldScope.Scope<WorldLoaderScope>().Get<WorldUnloader>().Run();
    }
}
