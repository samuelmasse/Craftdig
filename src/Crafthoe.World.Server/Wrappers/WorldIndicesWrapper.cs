namespace Crafthoe.World;

[World]
public class WorldIndicesWrapper(WorldModuleIndices moduleIndices)
{
    public const int Type = 5;

    private int cacheCount;
    private byte[] cache = [];

    public NetMessage Wrap()
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

        return new(Type, cache);
    }
}
