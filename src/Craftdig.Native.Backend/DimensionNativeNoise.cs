namespace Craftdig.Native;

[Dimension]
public class DimensionNativeNoise : IDisposable
{
    private const string SourceNodeName = "Simplex";
    private const string RootNodeName = "FractalFBm";
    private const float NoiseFrequency = 0.01f;
    private const float NoiseFeatureScale = 1f / NoiseFrequency;
    private const float FractalGain = 0.5f;
    private const float FractalBounding =
        1f /
        (1f +
         FractalGain +
         (FractalGain * FractalGain));

    private static readonly float[] RotatedSectionX = CreateRotatedSectionPositions(0);
    private static readonly float[] RotatedSectionY = CreateRotatedSectionPositions(1);
    private static readonly float[] RotatedSectionZ = CreateRotatedSectionPositions(2);

    private readonly Fn fn;
    private readonly ThreadLocal<FastNoise2NodePair> fastNoise2Nodes;
    private readonly int seed;
    private bool disposed;

    public DimensionNativeNoise(WorldMeta meta, Fn fn)
    {
        this.fn = fn;
        seed = meta.Seed;

        fastNoise2Nodes = new(CreateFastNoise2Nodes, true);
        WarmupFastNoise2();
    }

    public void GenerateSection(Span<float> noiseOut, int x, int y, int z)
    {
        TransformTerrainNoise3D(x, y, z, out var offsetX, out var offsetY, out var offsetZ);

        fn.GenPositionArray3D(
            fastNoise2Nodes.Value.Root,
            noiseOut,
            SectionVolume,
            RotatedSectionX,
            RotatedSectionY,
            RotatedSectionZ,
            offsetX,
            offsetY,
            offsetZ,
            seed);
    }

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;

        foreach (var nodes in fastNoise2Nodes.Values)
            nodes.Delete(fn);

        fastNoise2Nodes.Dispose();
    }

    private void WarmupFastNoise2()
    {
        Span<float> value = stackalloc float[1];
        fn.GenPositionArray3D(
            fastNoise2Nodes.Value.Root,
            value,
            1,
            RotatedSectionX,
            RotatedSectionY,
            RotatedSectionZ,
            0f,
            0f,
            0f,
            seed);
    }

    private FastNoise2NodePair CreateFastNoise2Nodes()
    {
        FnNode source = default;
        FnNode root = default;

        try
        {
            (source, _) = CreateNode(fn, SourceNodeName);
            SetVariableFloat(fn, source, "Feature Scale", NoiseFeatureScale);
            SetVariableInt(fn, source, "Seed Offset", 0);
            SetVariableFloat(fn, source, "Output Min", -FractalBounding);
            SetVariableFloat(fn, source, "Output Max", FractalBounding);

            (root, _) = CreateNode(fn, RootNodeName);
            SetVariableInt(fn, root, "Octaves", 3);
            SetVariableFloat(fn, root, "Lacunarity", 2f);
            SetHybridFloat(fn, root, "Gain", FractalGain);
            SetHybridFloat(fn, root, "Weighted Strength", 0f);
            SetNodeLookup(fn, root, "Source", source);

            return new(root, source);
        }
        catch
        {
            if (root != default)
                fn.DeleteNodeRef(root);

            if (source != default)
                fn.DeleteNodeRef(source);

            throw;
        }
    }

    private static (FnNode Node, string Name) CreateNode(Fn fn, string nodeName)
    {
        var exact = TryCreateNode(fn, name => string.Equals(name, nodeName, StringComparison.OrdinalIgnoreCase));
        if (exact.Node != default)
            return exact;

        var partial = TryCreateNode(fn, name => name.Contains(nodeName, StringComparison.OrdinalIgnoreCase));
        if (partial.Node != default)
            return partial;

        throw new InvalidOperationException($"FastNoise2 did not expose a creatable {nodeName} metadata node.");
    }

    private static (FnNode Node, string Name) TryCreateNode(Fn fn, Func<string, bool> match)
    {
        var count = fn.GetMetadataCount();
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataName(i, out var metadataName);
            var name = metadataName ?? string.Empty;
            if (!match(name))
                continue;

            var node = fn.NewFromMetadata(i, uint.MaxValue);
            if (node != default)
                return (node, name);
        }

        return (default, string.Empty);
    }

    private static void SetVariableFloat(Fn fn, FnNode node, string variableName, float value)
    {
        var index = FindVariableIndex(fn, node, variableName);
        if (!fn.SetVariableFloat(node, index, value))
            throw new InvalidOperationException($"FastNoise2 rejected float variable '{variableName}'.");
    }

    private static void SetVariableInt(Fn fn, FnNode node, string variableName, int value)
    {
        var index = FindVariableIndex(fn, node, variableName);
        if (!fn.SetVariableIntEnum(node, index, value))
            throw new InvalidOperationException($"FastNoise2 rejected int variable '{variableName}'.");
    }

    private static void SetHybridFloat(Fn fn, FnNode node, string hybridName, float value)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataHybridCount(metadataId);
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataHybridName(metadataId, i, out var name);
            if (!string.Equals(name, hybridName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!fn.SetHybridFloat(node, i, value))
                throw new InvalidOperationException($"FastNoise2 rejected hybrid '{hybridName}'.");

            return;
        }

        throw new InvalidOperationException($"FastNoise2 node does not expose hybrid '{hybridName}'.");
    }

    private static void SetNodeLookup(Fn fn, FnNode node, string lookupName, FnNode source)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataNodeLookupCount(metadataId);
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataNodeLookupName(metadataId, i, out var name);
            if (!string.Equals(name, lookupName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!fn.SetNodeLookup(node, i, source))
                throw new InvalidOperationException($"FastNoise2 rejected node lookup '{lookupName}'.");

            return;
        }

        throw new InvalidOperationException($"FastNoise2 node does not expose node lookup '{lookupName}'.");
    }

    private static int FindVariableIndex(Fn fn, FnNode node, string variableName)
    {
        var metadataId = fn.GetMetadataID(node);
        var count = fn.GetMetadataVariableCount(metadataId);
        for (var i = 0; i < count; i++)
        {
            fn.GetMetadataVariableName(metadataId, i, out var name);
            if (string.Equals(name, variableName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        throw new InvalidOperationException($"FastNoise2 node does not expose variable '{variableName}'.");
    }

    private static float[] CreateRotatedSectionPositions(int component)
    {
        var positions = new float[SectionVolume];

        for (int z = 0; z < SectionSize; z++)
        {
            int zIndex = z << (SectionBits * 2);
            for (int y = 0; y < SectionSize; y++)
            {
                int yzIndex = zIndex + (y << SectionBits);
                for (int x = 0; x < SectionSize; x++)
                {
                    TransformTerrainNoise3D(x, y, z, out var tx, out var ty, out var tz);

                    positions[yzIndex + x] = component switch
                    {
                        0 => tx,
                        1 => ty,
                        _ => tz,
                    };
                }
            }
        }

        return positions;
    }

    private static void TransformTerrainNoise3D(float x, float y, float z, out float tx, out float ty, out float tz)
    {
        const float r3 = 2f / 3f;
        var r = (x + y + z) * r3;
        tx = r - x;
        ty = r - y;
        tz = r - z;
    }

    private readonly record struct FastNoise2NodePair(FnNode Root, FnNode Source)
    {
        public void Delete(Fn fn)
        {
            if (Root != default)
                fn.DeleteNodeRef(Root);

            if (Source != default)
                fn.DeleteNodeRef(Source);
        }
    }
}
