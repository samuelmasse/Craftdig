namespace Craftdig;

[Player]
public class PlayerTestCubeMesher(RootCube cube, DimensionLights lights)
{
    public void Mesh(
        List<BlockVertex> vertices,
        Vec3d worldPosition,
        Vec3d origin,
        BlockFaces faces,
        float size)
    {
        var position = worldPosition.Swizzle();

        AddQuad(cube.Front, 0.8f, faces.Front.FaceIndex);
        AddQuad(cube.Back, 0.8f, faces.Back.FaceIndex);
        AddQuad(cube.Left, 0.6f, faces.Left.FaceIndex);
        AddQuad(cube.Right, 0.6f, faces.Right.FaceIndex);
        AddQuad(cube.Top, 1f, faces.Top.FaceIndex);
        AddQuad(cube.Bottom, 0.5f, faces.Bottom.FaceIndex);

        void AddQuad(CubeFace face, float shadow, int texture)
        {
            var quad = face.Quad;
            var offset = position - origin - new Vec3(size) / 2;
            var samplePosition = worldPosition + (Vec3d)face.Normal.Swizzle() * (size / 2 + 0.01f);
            var levels = lights.Get(samplePosition.ToLoc());
            var lighting = new Vec3(
                shadow,
                levels.Sky / (float)LightLevel.Max,
                levels.Block / (float)LightLevel.Max);
            vertices.Add(new((Vec3)(quad.TopLeft * size + offset), lighting, (0, 1, texture)));
            vertices.Add(new((Vec3)(quad.TopRight * size + offset), lighting, (1, 1, texture)));
            vertices.Add(new((Vec3)(quad.BottomLeft * size + offset), lighting, (0, 0, texture)));
            vertices.Add(new((Vec3)(quad.BottomRight * size + offset), lighting, (1, 0, texture)));
        }
    }
}
