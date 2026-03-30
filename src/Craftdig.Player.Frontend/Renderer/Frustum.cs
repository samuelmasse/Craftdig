namespace Craftdig.Player.Frontend;

public readonly struct Frustum(Matrix4 m)
{
    private readonly Vector4 left = Normalize(m.Column3 + m.Column0);
    private readonly Vector4 right = Normalize(m.Column3 - m.Column0);
    private readonly Vector4 bottom = Normalize(m.Column3 + m.Column1);
    private readonly Vector4 top = Normalize(m.Column3 - m.Column1);
    private readonly Vector4 near = Normalize(m.Column3 + m.Column2);
    private readonly Vector4 far = Normalize(m.Column3 - m.Column2);

    public bool Intersects(Box3 box) =>
        !IsOutside(left, box) &&
        !IsOutside(right, box) &&
        !IsOutside(bottom, box) &&
        !IsOutside(top, box) &&
        !IsOutside(near, box) &&
        !IsOutside(far, box);

    private static bool IsOutside(Vector4 plane, Box3 box) =>
        plane.X * (plane.X >= 0 ? box.Max.X : box.Min.X) +
        plane.Y * (plane.Y >= 0 ? box.Max.Y : box.Min.Y) +
        plane.Z * (plane.Z >= 0 ? box.Max.Z : box.Min.Z) +
        plane.W < 0;

    private static Vector4 Normalize(Vector4 plane) =>
        plane / plane.Xyz.Length;
}
