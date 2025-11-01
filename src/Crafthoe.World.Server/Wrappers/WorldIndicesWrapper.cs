namespace Crafthoe.World.Server;

[World]
public class WorldIndicesWrapper(WorldModuleIndices moduleIndices)
{
    private int cacheCount;
    private byte[] cache = [];

    public Span<byte> Wrap()
    {
        if (moduleIndices.Names.Length != cacheCount)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < moduleIndices.Names.Length; i++)
            {
                var name = moduleIndices.Names[i];
                if (i != 0)
                    sb.Append(',');
                sb.Append(name);
            }

            var csv = sb.ToString();

            cacheCount = moduleIndices.Names.Length;
            cache = Encoding.UTF8.GetBytes(csv);
        }

        return cache;
    }
}
