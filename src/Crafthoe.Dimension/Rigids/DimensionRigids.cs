namespace Crafthoe.Dimension;

[Dimension]
public class DimensionRigids(DimensionBlocks blocks, DimensionRigidBag rigidBag)
{
    public void Tick()
    {
        foreach (var ent in rigidBag.Ents)
        {
            ent.PrevPosition() = ent.Position();
            ent.CollisionNormal() = default;
            Collide((EntMut)ent);
            ent.Position() += ent.Velocity();

            if (ent.GetIsFlying())
            {
                double v = ent.Velocity().Z;
                ent.Velocity() *= (0.91f, 0.91f, 0.98f);
                ent.Velocity().Z = v * 0.6f;
            }
            else
            {
                ent.Velocity() *= (0.91f * 0.6f, 0.91f * 0.6f, 0.98f);
                ent.Velocity().Z -= 0.08;
            }
        }
    }

    private void Collide(EntMut ent)
    {
        for (var i = 0; i < 3; i++)
            CollideAxis(ent);
    }

    private void CollideAxis(EntMut ent)
    {
        ref var vel = ref ent.Velocity();
        ref var pos = ref ent.Position();

        var box = new Box3d(ent.HitBox().Min + pos, ent.HitBox().Max + pos);
        var tbox = new Box3d(box.Min + vel, box.Max + vel);

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

                    if (!blocks.TryGet(loc, out var block) || !block.IsSolid())
                        continue;

                    var bbox = new Box3d(loc, loc + Vector3i.One);
                    if (!Collide(box, vel, bbox, out var t, out var n) || t >= tmin)
                        continue;

                    tmin = t;
                    nmin = n;
                }
            }
        }

        if (tmin == double.PositiveInfinity)
            return;

        tmin -= 0.001;
        pos += vel * tmin * Vector3i.Abs(nmin);
        vel *= Vector3i.One - Vector3i.Abs(nmin);
        ent.CollisionNormal() = Vector3i.Clamp(ent.CollisionNormal() + nmin, -Vector3i.One, Vector3i.One);
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
