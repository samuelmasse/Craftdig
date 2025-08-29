namespace Crafthoe.World;

[WorldLoader]
public class WorldUnloader(WorldGlw gl, WorldEntPtrBag entPtrBag)
{
    public void Run()
    {
        Traverse(gl);

        foreach (var ent in entPtrBag.Ents)
            ent.Dispose();
    }

    private void Traverse(Glw gl)
    {
        for (int i = gl.Children.Count - 1; i >= 0; i--)
            Traverse(gl.Children[i]);

        gl.Dispose();
    }
}
