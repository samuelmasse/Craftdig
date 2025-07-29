namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionMesher(RootCube cube, DimensionBlocks blocks)
{
    private readonly List<PositionColorTextureVertex> vertices = [];

    public ReadOnlySpan<PositionColorTextureVertex> Vertices => CollectionsMarshal.AsSpan(vertices);

    public void Render(Vector3i sloc)
    {
        var loc = sloc * SectionSize;

        for (int z = 0; z < SectionSize; z++)
            for (int y = 0; y < SectionSize; y++)
                for (int x = 0; x < SectionSize; x++)
                    RenderBlock(loc + (x, y, z));
    }

    public void RenderBlock(Vector3i loc)
    {
        if (blocks.TryGet(loc, out var block) && blocks.TryGet(loc + (0, 0, 1), out var top) && block.IsSolid() != top.IsSolid())
        {
            var rloc = new Vector3i(loc.X, loc.Z, loc.Y);
            vertices.Add(new(cube.Top.Quad.TopLeft + rloc, (1, 1, 1), (0, 1)));
            vertices.Add(new(cube.Top.Quad.TopRight + rloc, (1, 1, 1), (1, 1)));
            vertices.Add(new(cube.Top.Quad.BottomLeft + rloc, (1, 1, 1), (0, 0)));
            vertices.Add(new(cube.Top.Quad.BottomRight + rloc, (1, 1, 1), (1, 0)));
        }
    }

    public void Reset() => vertices.Clear();
}
