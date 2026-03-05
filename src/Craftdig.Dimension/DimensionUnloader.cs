namespace Craftdig.Dimension;

[DimensionLoader]
public class DimensionUnloader(DimensionEntArena entArena)
{
    public void Run()
    {
        entArena.Arena.Dispose();
    }
}
