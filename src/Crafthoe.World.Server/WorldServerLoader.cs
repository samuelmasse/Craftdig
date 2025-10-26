namespace Crafthoe.World;

[WorldLoader]
public class WorldServerLoader(WorldModuleIndicesLoader moduleIndicesLoader)
{
    public void Run()
    {
        moduleIndicesLoader.Run();
    }
}
