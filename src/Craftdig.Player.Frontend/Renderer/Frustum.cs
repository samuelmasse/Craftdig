namespace Craftdig.Player.Frontend;

public readonly struct Frustum(Matrix4 m)
{
    private readonly Vector4 left = Normalize((m.M14 + m.M11, m.M24 + m.M21, m.M34 + m.M31, m.M44 + m.M41));
    private readonly Vector4 right = Normalize((m.M14 - m.M11, m.M24 - m.M21, m.M34 - m.M31, m.M44 - m.M41));
    private readonly Vector4 bottom = Normalize((m.M14 + m.M12, m.M24 + m.M22, m.M34 + m.M32, m.M44 + m.M42));
    private readonly Vector4 top = Normalize((m.M14 - m.M12, m.M24 - m.M22, m.M34 - m.M32, m.M44 - m.M42));
    private readonly Vector4 near = Normalize((m.M14 + m.M13, m.M24 + m.M23, m.M34 + m.M33, m.M44 + m.M43));
    private readonly Vector4 far = Normalize((m.M14 - m.M13, m.M24 - m.M23, m.M34 - m.M33, m.M44 - m.M43));

    public bool IsBoxVisible(Vector3 min, Vector3 max)
    {
        if (IsOutside(left, min, max)) return false;
        if (IsOutside(right, min, max)) return false;
        if (IsOutside(bottom, min, max)) return false;
        if (IsOutside(top, min, max)) return false;
        if (IsOutside(near, min, max)) return false;
        if (IsOutside(far, min, max)) return false;
        return true;
    }

    private bool IsOutside(Vector4 plane, Vector3 min, Vector3 max)
    {
        float px = plane.X >= 0 ? max.X : min.X;
        float py = plane.Y >= 0 ? max.Y : min.Y;
        float pz = plane.Z >= 0 ? max.Z : min.Z;
        return (plane.X * px + plane.Y * py + plane.Z * pz + plane.W) < 0;
    }

    private static Vector4 Normalize(Vector4 plane)
    {
        float mag = plane.Xyz.Length;
        return plane / mag;
    }
}
