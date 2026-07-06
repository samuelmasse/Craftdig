namespace Craftdig.World.Frontend;

[WorldLoader]
public class WorldFrontendUnloader(WorldGl gl)
{
    public void Run() => gl.Dispose();
}
