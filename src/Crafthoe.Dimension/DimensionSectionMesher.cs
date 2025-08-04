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
                    RenderBlock(loc, loc + (x, y, z));
    }

    public void RenderBlock(Vector3i origin, Vector3i loc)
    {
        blocks.TryGet(loc, out var block);
        if (!block.IsSolid())
            return;

        var rloc = new Vector3i(loc.X, loc.Z, loc.Y) - (origin.X, origin.Z, origin.Y);

        blocks.TryGet(loc + (0, 1, 0), out var front);
        blocks.TryGet(loc - (0, 1, 0), out var back);

        blocks.TryGet(loc + (1, 0, 0), out var right);
        blocks.TryGet(loc - (1, 0, 0), out var left);

        blocks.TryGet(loc + (0, 0, 1), out var top);
        blocks.TryGet(loc - (0, 0, 1), out var bottom);

        if (!front.IsSolid())
            AddQuad(cube.Front.Quad, 1);
        if (!back.IsSolid())
            AddQuad(cube.Back.Quad, 1);

        if (!left.IsSolid())
            AddQuad(cube.Left.Quad, 0.5f);
        if (!right.IsSolid())
            AddQuad(cube.Right.Quad, 0.5f);

        if (!top.IsSolid())
            AddQuad(cube.Top.Quad, 0.8f);
        if (!bottom.IsSolid())
            AddQuad(cube.Bottom.Quad, 0.8f);

        void AddQuad(Quad quad, float shadow)
        {
            vertices.Add(new(quad.TopLeft + rloc, Vector3.One * shadow, (0, 1)));
            vertices.Add(new(quad.TopRight + rloc, Vector3.One * shadow, (1, 1)));
            vertices.Add(new(quad.BottomLeft + rloc, Vector3.One * shadow, (0, 0)));
            vertices.Add(new(quad.BottomRight + rloc, Vector3.One * shadow, (1, 0)));
        }
    }

    public void Reset() => vertices.Clear();
}
