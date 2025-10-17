namespace Crafthoe.World;

[WorldLoader]
public class WorldLoader(WorldModuleIndicesLoader moduleIndicesLoader)
{
    public void Run()
    {
        moduleIndicesLoader.Run();
    }
}
