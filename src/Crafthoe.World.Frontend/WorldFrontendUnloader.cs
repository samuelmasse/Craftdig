namespace Crafthoe.World.Frontend;

[WorldLoader]
public class WorldFrontendUnloader(WorldGlw gl)
{
    public void Run()
    {
        Traverse(gl);
    }

    private void Traverse(Glw gl)
    {
        for (int i = gl.Children.Count - 1; i >= 0; i--)
            Traverse(gl.Children[i]);

        gl.Dispose();
    }
}
