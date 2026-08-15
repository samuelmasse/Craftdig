namespace Craftdig;

[Player]
public class PlayerBodyMesher(
    RootCube cube,
    ModuleFaceAtlas faceAtlas,
    DimensionLights lights)
{
    private readonly int bodyTexture = faceAtlas["Stone"];
    private readonly int headTexture = faceAtlas["Dirt"];
    private readonly int faceTexture = faceAtlas["Glowstone"];

    public void Mesh(
        List<BlockVertex> vertices,
        Vec3d worldPosition,
        Vec3 lookAt,
        Vec3 origin)
    {
        var position = (Vec3)worldPosition.Swizzle();
        var levels = lights.Get(worldPosition.ToLoc());
        float sky = levels.Sky / (float)LightLevel.Max;
        float block = levels.Block / (float)LightLevel.Max;

        AddBox(
            position - Vec3.UnitY,
            (0.6f, 1.2f, 0.35f),
            Vec3.UnitX,
            Vec3.UnitY,
            Vec3.UnitZ,
            bodyTexture,
            bodyTexture);

        var forward = lookAt.Swizzle().NormalizedOr(Vec3.UnitZ);
        var right = Vec3.Cross(Vec3.UnitY, forward).NormalizedOr(Vec3.UnitX);
        var up = Vec3.Cross(forward, right).NormalizedOr(Vec3.UnitY);
        AddBox(
            position - Vec3.UnitY * 0.12f,
            (0.5f, 0.5f, 0.6f),
            right,
            up,
            forward,
            headTexture,
            faceTexture);

        void AddBox(
            Vec3 center,
            Vec3 size,
            Vec3 right,
            Vec3 up,
            Vec3 forward,
            int texture,
            int frontTexture)
        {
            AddQuad(cube.Front, frontTexture);
            AddQuad(cube.Back, texture);
            AddQuad(cube.Left, texture);
            AddQuad(cube.Right, texture);
            AddQuad(cube.Top, texture);
            AddQuad(cube.Bottom, texture);

            void AddQuad(CubeFace face, int faceTexture)
            {
                var normal = right * face.Normal.X + up * face.Normal.Y + forward * face.Normal.Z;
                float shadow = normal.Y > 0.5f ? 1f : normal.Y < -0.5f ? 0.5f : 0.7f;
                var lighting = new Vec3(shadow, sky, block);
                var quad = face.Quad;
                vertices.Add(new(Vertex(quad.TopLeft), lighting, (0, 1, faceTexture)));
                vertices.Add(new(Vertex(quad.TopRight), lighting, (1, 1, faceTexture)));
                vertices.Add(new(Vertex(quad.BottomLeft), lighting, (0, 0, faceTexture)));
                vertices.Add(new(Vertex(quad.BottomRight), lighting, (1, 0, faceTexture)));
            }

            Vec3 Vertex(Vec3 corner)
            {
                var local = (corner - new Vec3(0.5f)) * size;
                return center - origin + right * local.X + up * local.Y + forward * local.Z;
            }
        }
    }
}
