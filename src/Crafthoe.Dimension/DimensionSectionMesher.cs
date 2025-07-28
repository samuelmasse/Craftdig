namespace Crafthoe.Dimension;

[Dimension]
public class DimensionSectionMesher(RootCube cube, DimensionChunks chunks, DimensionSections sections, DimensionBlocks blocks)
{
    private readonly List<PositionColorTextureVertex> vertices = [];

    public ReadOnlySpan<PositionColorTextureVertex> Vertices => CollectionsMarshal.AsSpan(vertices);

    public void Render(Vector3i sloc)
    {
        var loc = sloc * sections.Unit;

        for (int z = 0; z < chunks.Unit.Z; z++)
            for (int y = 0; y < chunks.Unit.Y; y++)
                for (int x = 0; x < chunks.Unit.X; x++)
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
