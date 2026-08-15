namespace Craftdig;

[Player]
public class PlayerViewModelMesher(RootCube cube, ModuleFaceAtlas faceAtlas)
{
    public const int MaxVertices = 24;

    public static readonly Vec3 ArmShoulderRest = (0.48f, -0.84f, -0.53f);
    private static readonly Vec3 ArmSize = (0.25f, 0.75f, 0.25f);
    private static readonly Vec3 ArmShoulderPivot = (0.5f, 0, 0.5f);
    private static readonly Quat ArmRestOrientation =
        Quat.CreateFromAxisAngle(Vec3.UnitY, Radians(45)) *
        Quat.CreateFromAxisAngle(Vec3.UnitZ, Radians(120)) *
        Quat.CreateFromAxisAngle(Vec3.UnitX, Radians(200)) *
        Quat.CreateFromAxisAngle(Vec3.UnitY, Radians(-135)) *
        Quat.CreateFromAxisAngle(Vec3.UnitZ, 0.1f);

    private static readonly Vec3 BlockCenterRest = (0.56f, -0.52f, -0.72f);
    private static readonly Vec3 BlockCenterPivot = new(0.5f);
    private static readonly float BlockRestYaw = Radians(45);
    private const float BlockScale = 0.4f;

    private const float ColumnWidth = 0.25f;
    private const float CapV = 0.75f;
    private const int InnerColumn = 0;
    private const int UnderColumn = 1;
    private const int OuterColumn = 2;
    private const int LitColumn = 3;
    private const int ShoulderCapColumn = 0;
    private const int FistCapColumn = 1;

    private readonly int armTexture = faceAtlas["PlayerArm"];

    public int Mesh(
        Span<BlockVertex> vertices,
        PlayerViewModelPose pose,
        Ent item,
        float sky,
        float block)
    {
        int count = 0;
        if (item != default && item.IsBuildable)
            MeshBlock(vertices, ref count, pose, item.Faces, sky, block);
        else
            MeshArm(vertices, ref count, pose, sky, block);
        return count;
    }

    private void MeshArm(
        Span<BlockVertex> vertices,
        ref int count,
        PlayerViewModelPose pose,
        float sky,
        float block)
    {
        var origin = ArmShoulderRest + pose.Offset.Swizzle();
        var rotation = PoseRotation(pose.Rotation) * ArmRestOrientation;

        AddArmSide(vertices, ref count, cube.Left, origin, rotation, LitColumn, sky, block);
        AddArmSide(vertices, ref count, cube.Back, origin, rotation, InnerColumn, sky, block);
        AddArmSide(vertices, ref count, cube.Front, origin, rotation, OuterColumn, sky, block);
        AddArmSide(vertices, ref count, cube.Right, origin, rotation, UnderColumn, sky, block);
        AddArmCap(vertices, ref count, cube.Top, origin, rotation, FistCapColumn, sky, block);
        AddArmCap(vertices, ref count, cube.Bottom, origin, rotation, ShoulderCapColumn, sky, block);
    }

    private void AddArmSide(
        Span<BlockVertex> vertices,
        ref int count,
        CubeFace face,
        Vec3 origin,
        Quat rotation,
        int column,
        float sky,
        float block)
    {
        float u0 = column * ColumnWidth;
        float u1 = u0 + ColumnWidth;
        AddQuad(
            vertices, ref count, face, ArmSize, ArmShoulderPivot, origin, rotation, armTexture,
            (u0, 0), (u1, 0), (u0, CapV), (u1, CapV), sky, block);
    }

    private void AddArmCap(
        Span<BlockVertex> vertices,
        ref int count,
        CubeFace face,
        Vec3 origin,
        Quat rotation,
        int column,
        float sky,
        float block)
    {
        float u0 = column * ColumnWidth;
        float u1 = u0 + ColumnWidth;
        AddQuad(
            vertices, ref count, face, ArmSize, ArmShoulderPivot, origin, rotation, armTexture,
            (u0, CapV), (u1, CapV), (u0, 1), (u1, 1), sky, block);
    }

    private void MeshBlock(
        Span<BlockVertex> vertices,
        ref int count,
        PlayerViewModelPose pose,
        BlockFaces faces,
        float sky,
        float block)
    {
        var origin = BlockCenterRest + pose.Offset.Swizzle();
        var rotation = BlockRotation(pose.Rotation);
        var size = new Vec3(BlockScale);
        var uv = new Vec4(0, 0, 1, 1);

        AddQuad(vertices, ref count, cube.Top, size, BlockCenterPivot, origin, rotation, faces.Top.FaceIndex, uv, sky, block);
        AddQuad(vertices, ref count, cube.Bottom, size, BlockCenterPivot, origin, rotation, faces.Bottom.FaceIndex, uv, sky, block);
        AddQuad(vertices, ref count, cube.Front, size, BlockCenterPivot, origin, rotation, faces.Front.FaceIndex, uv, sky, block);
        AddQuad(vertices, ref count, cube.Back, size, BlockCenterPivot, origin, rotation, faces.Back.FaceIndex, uv, sky, block);
        AddQuad(vertices, ref count, cube.Left, size, BlockCenterPivot, origin, rotation, faces.Left.FaceIndex, uv, sky, block);
        AddQuad(vertices, ref count, cube.Right, size, BlockCenterPivot, origin, rotation, faces.Right.FaceIndex, uv, sky, block);
    }

    private void AddQuad(
        Span<BlockVertex> vertices,
        ref int count,
        CubeFace face,
        Vec3 size,
        Vec3 pivot,
        Vec3 origin,
        Quat rotation,
        int textureLayer,
        Vec4 uv,
        float sky,
        float block)
    {
        AddQuad(
            vertices,
            ref count,
            face,
            size,
            pivot,
            origin,
            rotation,
            textureLayer,
            (uv.X, uv.W),
            (uv.Z, uv.W),
            (uv.X, uv.Y),
            (uv.Z, uv.Y),
            sky,
            block);
    }

    private void AddQuad(
        Span<BlockVertex> vertices,
        ref int count,
        CubeFace face,
        Vec3 size,
        Vec3 pivot,
        Vec3 origin,
        Quat rotation,
        int textureLayer,
        Vec2 topLeftUv,
        Vec2 topRightUv,
        Vec2 bottomLeftUv,
        Vec2 bottomRightUv,
        float sky,
        float block)
    {
        var normal = rotation * face.Normal;
        float shadow = normal.Y > 0.5f ? 1 : normal.Y < -0.5f ? 0.5f : 0.72f;
        var lighting = new Vec3(shadow, sky, block);
        var quad = face.Quad;

        vertices[count++] = new(Vertex(quad.TopLeft), lighting, (topLeftUv.X, topLeftUv.Y, textureLayer));
        vertices[count++] = new(Vertex(quad.TopRight), lighting, (topRightUv.X, topRightUv.Y, textureLayer));
        vertices[count++] = new(Vertex(quad.BottomLeft), lighting, (bottomLeftUv.X, bottomLeftUv.Y, textureLayer));
        vertices[count++] = new(Vertex(quad.BottomRight), lighting, (bottomRightUv.X, bottomRightUv.Y, textureLayer));

        Vec3 Vertex(Vec3 corner) => rotation * ((corner - pivot) * size) + origin;
    }

    private Quat PoseRotation(Vec3 rotation) =>
        Quat.CreateFromAxisAngle(Vec3.UnitY, -rotation.Z) *
        Quat.CreateFromAxisAngle(Vec3.UnitX, -rotation.X) *
        Quat.CreateFromAxisAngle(Vec3.UnitZ, -rotation.Y);

    private Quat BlockRotation(Vec3 rotation) =>
        Quat.CreateFromAxisAngle(Vec3.UnitY, BlockRestYaw - rotation.Z) *
        Quat.CreateFromAxisAngle(Vec3.UnitZ, -rotation.Y) *
        Quat.CreateFromAxisAngle(Vec3.UnitX, -rotation.X);

    private static float Radians(float degrees) => degrees * (float.Pi / 180);
}
