namespace Crafthoe.World.Backend;

[WorldLoader]
public class WorldServerLoader(WorldModuleIndicesLoader moduleIndicesLoader)
{
    public void Run()
    {
        moduleIndicesLoader.Run();
    }
}
