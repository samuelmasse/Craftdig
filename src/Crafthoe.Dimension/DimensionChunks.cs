namespace Crafthoe.Dimension;

[Dimension]
public class DimensionChunks
{
    private readonly Vector3i unit = (16, 16, 384);
    private readonly Dictionary<Vector2i, Entity> dict = [];

    public Vector3i Unit => unit;

    public Entity? this[Vector2i cloc]
    {
        get
        {
            if (dict.TryGetValue(cloc, out var array))
                return array;
            else return null;
        }
    }

    public void Alloc(Vector2i cloc)
    {
        if (dict.ContainsKey(cloc))
            return;

        var chunk = new Entity()
            .IsChunk(true)
            .ChunkLocation(cloc);

        dict.Add(cloc, chunk);
    }

    public void Free(Vector2i cloc)
    {
        if (!dict.TryGetValue(cloc, out var entity))
            return;

        entity.Free();
        dict.Remove(cloc);
    }
}
