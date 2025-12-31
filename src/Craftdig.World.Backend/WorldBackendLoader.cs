namespace Craftdig.World.Backend;

[WorldLoader]
public class WorldBackendLoader(WorldModuleIndicesLoader moduleIndicesLoader)
{
    public void Run()
    {
        moduleIndicesLoader.Run();
    }
}
