namespace Craftdig;

[DimensionLoader]
public class DimensionUnloader(DimensionEntArena entArena, DimensionChunkArena chunkArena)
{
    public void Run()
    {
        entArena.Dispose();
        chunkArena.Dispose();
    }
}
