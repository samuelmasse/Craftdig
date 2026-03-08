namespace Craftdig.Dimension;

[Dimension]
public class DimensionRigids(DimensionBlocks blocks, DimensionRigidBag rigidBag)
{
    public void Tick()
    {
        foreach (var ent in rigidBag.Ents)
            Tick(ent);
    }

    private void Tick(EntMutIdx ent)
    {
        ent.PrevPosition = ent.Position;
        ent.CollisionNormal = default;

        var position = ent.Position;
        var velocity = ent.Velocity;
        var hitBox = ent.HitBox;
        var collisionNormal = ent.CollisionNormal;
        Collide(ref position, ref velocity, ref hitBox, ref collisionNormal);

        position += velocity;

        if (ent.IsProjectile)
        {
            float drag = 1;
            if (collisionNormal.Z == 1)
                drag = 0.6f;
            velocity *= 0.98 * drag;
            velocity -= (0, 0, 0.08);
        }
        else if (ent.IsFlying)
        {
            double v = velocity.Z;
            velocity *= (0.91f, 0.91f, 0.98f);
            velocity = velocity with { Z = v * 0.6f };
        }
        else
        {
            velocity *= (0.91f * 0.6f, 0.91f * 0.6f, 0.98f);
            velocity -= (0, 0, 0.08);
        }

        if (velocity.Xy.Length < 0.0001)
            velocity.Xy = default;
        if (Math.Abs(velocity.Z) < 0.0001)
            velocity.Z = 0;

        ent.Position = position;
        ent.Velocity = velocity;
        ent.HitBox = hitBox;
        ent.CollisionNormal = collisionNormal;
    }

    private void Collide(ref Vector3d position, ref Vector3d velocity, ref Box3d hitBox, ref Vector3i collisionNormal)
    {
        for (var i = 0; i < 3; i++)
            CollideAxis(ref position, ref velocity, ref hitBox, ref collisionNormal);
    }

    private void CollideAxis(ref Vector3d position, ref Vector3d velocity, ref Box3d hitBox, ref Vector3i collisionNormal)
    {
        var box = new Box3d(hitBox.Min + position, hitBox.Max + position);
        var tbox = new Box3d(box.Min + velocity, box.Max + velocity);

        var smin = Vector3d.ComponentMin(box.Min, tbox.Min).ToLoc() - Vector3i.One;
        var smax = Vector3d.ComponentMax(box.Max, tbox.Max).ToLoc() + Vector3i.One;

        double tmin = double.PositiveInfinity;
        Vector3i nmin = default;

        for (int z = smin.Z; z <= smax.Z; z++)
        {
            for (int x = smin.X; x <= smax.X; x++)
            {
                for (int y = smin.Y; y <= smax.Y; y++)
                {
                    var loc = new Vector3i(x, y, z);

                    if (!blocks.TryGet(loc, out var block) || !block.IsSolid)
                        continue;

                    var bbox = new Box3d(loc, loc + Vector3i.One);
                    if (!Collide(box, velocity, bbox, out var t, out var n) || t >= tmin)
                        continue;

                    tmin = t;
                    nmin = n;
                }
            }
        }

        if (tmin == double.PositiveInfinity)
            return;

        tmin -= 0.001;
        position += velocity * tmin * Vector3i.Abs(nmin);
        velocity *= Vector3i.One - Vector3i.Abs(nmin);
        collisionNormal = Vector3i.Clamp(collisionNormal + nmin, -Vector3i.One, Vector3i.One);
    }

    private static bool Collide(Box3d moving, Vector3d vel, Box3d solid, out double time, out Vector3i normal)
    {
        time = 1.0;
        normal = default;

        static (double entry, double exit) Axis(double minA, double maxA, double minB, double maxB, double v)
        {
            if (v > 0)
                return ((minB - maxA) / v, (maxB - minA) / v);
            if (v < 0)
                return ((maxB - minA) / v, (minB - maxA) / v);

            return (minA < maxB && maxA > minB ? double.NegativeInfinity : double.PositiveInfinity, double.PositiveInfinity);
        }

        var (xEntry, xExit) = Axis(moving.Min.X, moving.Max.X, solid.Min.X, solid.Max.X, vel.X);
        var (yEntry, yExit) = Axis(moving.Min.Y, moving.Max.Y, solid.Min.Y, solid.Max.Y, vel.Y);
        var (zEntry, zExit) = Axis(moving.Min.Z, moving.Max.Z, solid.Min.Z, solid.Max.Z, vel.Z);

        double entry = Math.Max(xEntry, Math.Max(yEntry, zEntry));
        double exit = Math.Min(xExit, Math.Min(yExit, zExit));

        if (entry > exit || entry < 0 || entry > 1)
            return false;

        if (entry == xEntry) normal.X = vel.X > 0 ? -1 : 1;
        else if (entry == yEntry) normal.Y = vel.Y > 0 ? -1 : 1;
        else if (entry == zEntry) normal.Z = vel.Z > 0 ? -1 : 1;

        time = entry;
        return true;
    }
}
