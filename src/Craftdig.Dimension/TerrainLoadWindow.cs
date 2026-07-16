namespace Craftdig.Dimension;

public readonly record struct TerrainLoadWindow(Vec2i Center, int SideLength)
{
    public int Radius => SideLength / 2;
    public int Count => SideLength * SideLength;

    public Vec2i this[int index]
    {
        get
        {
            int radius = Radius;
            return Center + (index % SideLength - radius, index / SideLength - radius);
        }
    }
}
