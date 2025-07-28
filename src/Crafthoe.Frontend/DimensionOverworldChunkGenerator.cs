
namespace Crafthoe.Frontend;

[Dimension]
public class DimensionOverworldChunkGenerator(
    ModuleBlocks block,
    DimensionChunks chunks,
    DimensionBlocks blocks) : IChunkGenerator
{
    private readonly FastNoiseLite noise = new();

    public void Generate(Vector2i cloc)
    {
        Console.WriteLine(cloc);
        var loc = new Vector3i(cloc.X * chunks.Unit.X, cloc.Y * chunks.Unit.Y, 0);

        for (int y = 0; y < chunks.Unit.Y; y++)
        {
            for (int x = 0; x < chunks.Unit.X; x++)
            {
                int h = (int)(noise.GetNoise(loc.X + x, loc.Y + y) * 15) + 60;

                for (int z = 0; z < chunks.Unit.Z; z++)
                    blocks.TrySet(loc + (x, y, z), Generate(loc + (x, y, z), h));
            }
        }
    }

    private ReadOnlyEntity Generate(Vector3i loc, float h)
    {
        if (loc.Z < h - 5)
            return block.Stone;
        else if (loc.Z < h)
            return block.Dirt;
        else return block.Air;
    }
}
