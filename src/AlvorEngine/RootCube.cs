namespace AlvorEngine;

[Root]
public class RootCube
{
    public readonly CubeFace Front = new(new((0, 1, 1), (1, 1, 1), (0, 0, 1), (1, 0, 1)), (0, 0, 1));
    public readonly CubeFace Back = new(new((1, 1, 0), (0, 1, 0), (1, 0, 0), (0, 0, 0)), (0, 0, -1));
    public readonly CubeFace Top = new(new((0, 1, 0), (1, 1, 0), (0, 1, 1), (1, 1, 1)), (0, 1, 0));
    public readonly CubeFace Bottom = new(new((0, 0, 1), (1, 0, 1), (0, 0, 0), (1, 0, 0)), (0, -1, 0));
    public readonly CubeFace Left = new(new((0, 1, 0), (0, 1, 1), (0, 0, 0), (0, 0, 1)), (-1, 0, 0));
    public readonly CubeFace Right = new(new((1, 1, 1), (1, 1, 0), (1, 0, 1), (1, 0, 0)), (1, 0, 0));
    private readonly CubeFace[] faces;

    public ReadOnlySpan<CubeFace> Faces => faces;

    public RootCube() => faces = [Front, Back, Top, Bottom, Left, Right];
}
