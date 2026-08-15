namespace Craftdig;

[Dimension]
public class DimensionSectionMesher(RootCube cube, DimensionBlocks blocks, DimensionLights lights)
{
    private readonly int sampleSize = SectionSize + 2;
    private readonly int sampleArea = (SectionSize + 2) * (SectionSize + 2);

    public void Render(List<BlockVertex> vertices, Vec3i sloc, SectionThreadSamples samples)
    {
        var origin = sloc * SectionSize;
        var blockSamples = samples.Blocks;
        var lightSamples = samples.Lights;

        CacheSamples(origin, blockSamples, lightSamples);

        for (int z = 0; z < SectionSize; z++)
            for (int y = 0; y < SectionSize; y++)
                for (int x = 0; x < SectionSize; x++)
                    RenderBlock(vertices, x, y, z, blockSamples, lightSamples);
    }

    private void CacheSamples(Vec3i origin, Ent[] blockSamples, LightLevels[] lightSamples)
    {
        for (int z = -1; z <= SectionSize; z++)
        {
            for (int y = -1; y <= SectionSize; y++)
            {
                for (int x = -1; x <= SectionSize; x++)
                {
                    int index = SampleIndex(x, y, z);
                    var loc = origin + (x, y, z);
                    blocks.TryGet(loc, out blockSamples[index]);
                    lights.TryGet(loc, out lightSamples[index]);
                }
            }
        }
    }

    private void RenderBlock(
        List<BlockVertex> vertices,
        int x,
        int y,
        int z,
        Ent[] blockSamples,
        LightLevels[] lightSamples)
    {
        var block = blockSamples[SampleIndex(x, y, z)];
        if (!block.IsSolid)
            return;

        var rloc = new Vec3i(x, z, y);
        var faces = block.Faces;

        if (!blockSamples[SampleIndex(x, y + 1, z)].IsSolid)
            AddQuad(cube.Front.Quad, 0.8f, faces.Front.FaceIndex, (0, 1, 0), (0, 0, 1), (1, 0, 0));
        if (!blockSamples[SampleIndex(x, y - 1, z)].IsSolid)
            AddQuad(cube.Back.Quad, 0.8f, faces.Back.FaceIndex, (0, -1, 0), (0, 0, 1), (-1, 0, 0));
        if (!blockSamples[SampleIndex(x - 1, y, z)].IsSolid)
            AddQuad(cube.Left.Quad, 0.6f, faces.Left.FaceIndex, (-1, 0, 0), (0, 0, 1), (0, 1, 0));
        if (!blockSamples[SampleIndex(x + 1, y, z)].IsSolid)
            AddQuad(cube.Right.Quad, 0.6f, faces.Right.FaceIndex, (1, 0, 0), (0, 0, 1), (0, -1, 0));
        if (!blockSamples[SampleIndex(x, y, z + 1)].IsSolid)
            AddQuad(cube.Top.Quad, 1f, faces.Top.FaceIndex, (0, 0, 1), (0, -1, 0), (1, 0, 0));
        if (!blockSamples[SampleIndex(x, y, z - 1)].IsSolid)
            AddQuad(cube.Bottom.Quad, 0.5f, faces.Bottom.FaceIndex, (0, 0, -1), (0, 1, 0), (1, 0, 0));

        void AddQuad(Quad3 quad, float shadow, int texture, Vec3i normal, Vec3i up, Vec3i right)
        {
            var outside = new Vec3i(x, y, z) + normal;
            var centerLight = lightSamples[SampleIndex(outside.X, outside.Y, outside.Z)];
            var (topLeft, topLeftAo) = CreateVertex(quad.TopLeft, 1, -1, (0, 1, texture));
            var (topRight, topRightAo) = CreateVertex(quad.TopRight, 1, 1, (1, 1, texture));
            var (bottomLeft, bottomLeftAo) = CreateVertex(quad.BottomLeft, -1, -1, (0, 0, texture));
            var (bottomRight, bottomRightAo) = CreateVertex(quad.BottomRight, -1, 1, (1, 0, texture));

            if (topLeftAo + bottomRightAo > topRightAo + bottomLeftAo)
            {
                vertices.Add(bottomLeft);
                vertices.Add(topLeft);
                vertices.Add(bottomRight);
                vertices.Add(topRight);
            }
            else
            {
                vertices.Add(topLeft);
                vertices.Add(topRight);
                vertices.Add(bottomLeft);
                vertices.Add(bottomRight);
            }

            (BlockVertex Vertex, int AmbientOcclusion) CreateVertex(
                Vec3 position,
                int upSign,
                int rightSign,
                Vec3 texCoord)
            {
                var upLoc = outside + up * upSign;
                var rightLoc = outside + right * rightSign;
                var cornerLoc = upLoc + right * rightSign;
                bool upOccludes = Occludes(upLoc);
                bool rightOccludes = Occludes(rightLoc);
                bool cornerOccludes = Occludes(cornerLoc);

                var upLight = upOccludes ? centerLight : Light(upLoc);
                var rightLight = rightOccludes ? centerLight : Light(rightLoc);
                var cornerLight = cornerOccludes || upOccludes && rightOccludes ? centerLight : Light(cornerLoc);
                int ambientOcclusion = upOccludes && rightOccludes
                    ? 0
                    : 3 - (upOccludes ? 1 : 0) - (rightOccludes ? 1 : 0) - (cornerOccludes ? 1 : 0);
                float ambientFactor = ambientOcclusion switch
                {
                    3 => 1f,
                    2 => 0.8f,
                    1 => 0.65f,
                    _ => 0.5f
                };

                float sky = (centerLight.Sky + upLight.Sky + rightLight.Sky + cornerLight.Sky) /
                    (LightLevel.Max * 4f);
                float blockLight = (centerLight.Block + upLight.Block + rightLight.Block + cornerLight.Block) /
                    (LightLevel.Max * 4f);
                return (new(position + rloc, (shadow * ambientFactor, sky, blockLight), texCoord), ambientOcclusion);
            }

            bool Occludes(Vec3i sample) =>
                blockSamples[SampleIndex(sample.X, sample.Y, sample.Z)].LightOpacity >= LightLevel.Max;
            LightLevels Light(Vec3i sample) => lightSamples[SampleIndex(sample.X, sample.Y, sample.Z)];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int SampleIndex(int x, int y, int z) =>
        x + 1 + (y + 1) * sampleSize + (z + 1) * sampleArea;
}
