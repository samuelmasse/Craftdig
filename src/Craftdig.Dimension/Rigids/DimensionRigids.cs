namespace Craftdig;

[Dimension]
public class DimensionRigids(DimensionBlocks blocks, DimensionRigidBag rigidBag)
{
    private const double CrouchBackoffStep = 0.05;
    private const double CrouchSupportDepth = 0.6;

    public void Tick()
    {
        foreach (var ent in rigidBag.Ents)
            if (!ent.IsRemote)
                Tick(ent);
    }

    private void Tick(EntMutIdx ent)
    {
        bool wasGrounded = ent.CollisionNormal.Z == 1;
        ent.PrevPosition = ent.Position;
        ent.CollisionNormal = default;

        var position = ent.Position;
        var velocity = ent.Velocity;
        var hitBox = ent.HitBox;
        var collisionNormal = ent.CollisionNormal;
        PreventCrouchFall(ent, wasGrounded, position, hitBox, ref velocity);
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

    private void PreventCrouchFall(
        EntMutIdx ent,
        bool wasGrounded,
        Vec3d position,
        Box3d hitBox,
        ref Vec3d velocity)
    {
        if (!ent.IsPlayer || !ent.IsCrouching || ent.IsFlying || !wasGrounded || velocity.Z > 0)
            return;

        double x = velocity.X;
        double y = velocity.Y;

        while (x != 0 && !HasCrouchSupport(position, hitBox, x, 0))
            x = BackOff(x);

        while (y != 0 && !HasCrouchSupport(position, hitBox, 0, y))
            y = BackOff(y);

        while (x != 0 && y != 0 && !HasCrouchSupport(position, hitBox, x, y))
        {
            x = BackOff(x);
            y = BackOff(y);
        }

        velocity.X = x;
        velocity.Y = y;
    }

    private bool HasCrouchSupport(Vec3d position, Box3d hitBox, double x, double y)
    {
        var box = hitBox.Translated(position + (x, y, -CrouchSupportDepth));
        var smin = box.Min.ToLoc();
        var smax = box.Max.ToLoc();

        for (int z = smin.Z; z <= smax.Z; z++)
        {
            for (int xLoc = smin.X; xLoc <= smax.X; xLoc++)
            {
                for (int yLoc = smin.Y; yLoc <= smax.Y; yLoc++)
                {
                    var loc = new Vec3i(xLoc, yLoc, z);
                    if (!blocks.TryGet(loc, out var block) || !block.IsSolid)
                        continue;

                    if (box.IntersectsExclusive(new Box3d(loc, loc + Vec3i.One)))
                        return true;
                }
            }
        }

        return false;
    }

    private static double BackOff(double value)
    {
        if (value < CrouchBackoffStep && value >= -CrouchBackoffStep)
            return 0;

        return value > 0 ? value - CrouchBackoffStep : value + CrouchBackoffStep;
    }

    private void Collide(ref Vec3d position, ref Vec3d velocity, ref Box3d hitBox, ref Vec3i collisionNormal)
    {
        var size = hitBox.Max - hitBox.Min;

        if (size.X <= 1 && size.Y <= 1 && velocity.X == 0 && velocity.Y == 0 && velocity.Z < 0)
        {
            // We can early exit if only pulled by gravity and any of the 4 corners touch a solid block
            if (TryCollideBottom(ref position, ref velocity, hitBox, ref collisionNormal))
                return;
        }

        for (var i = 0; i < 3; i++)
            CollideAxis(ref position, ref velocity, ref hitBox, ref collisionNormal);
    }

    private bool TryCollideBottom(ref Vec3d position, ref Vec3d velocity, Box3d hitBox, ref Vec3i collisionNormal)
    {
        var box = new Box3d(hitBox.Min + position, hitBox.Max + position);
        var tbox = new Box3d(box.Min + velocity, box.Max + velocity);

        int z = (int)Math.Floor(tbox.Min.Z);
        double minX = box.Min.X;
        double maxX = box.Max.X - 0.000001;
        double minY = box.Min.Y;
        double maxY = box.Max.Y - 0.000001;

        if (IsSolidCorner((minX, minY, z)) || IsSolidCorner((minX, maxY, z)) ||
            IsSolidCorner((maxX, minY, z)) || IsSolidCorner((maxX, maxY, z)))
        {
            position.Z = z + 1 - hitBox.Min.Z;
            velocity.Z = 0;
            collisionNormal = (0, 0, 1);
            return true;
        }

        return false;
    }

    private bool IsSolidCorner(Vec3d pos) => blocks.TryGet(pos.ToLoc(), out var block) && block.IsSolid;

    private void CollideAxis(ref Vec3d position, ref Vec3d velocity, ref Box3d hitBox, ref Vec3i collisionNormal)
    {
        var box = new Box3d(hitBox.Min + position, hitBox.Max + position);
        var tbox = new Box3d(box.Min + velocity, box.Max + velocity);

        var smin = Vec3d.Min(box.Min, tbox.Min).ToLoc() - Vec3i.One;
        var smax = Vec3d.Max(box.Max, tbox.Max).ToLoc() + Vec3i.One;

        double tmin = double.PositiveInfinity;
        Vec3i nmin = default;

        for (int z = smin.Z; z <= smax.Z; z++)
        {
            for (int x = smin.X; x <= smax.X; x++)
            {
                for (int y = smin.Y; y <= smax.Y; y++)
                {
                    var loc = new Vec3i(x, y, z);

                    if (!blocks.TryGet(loc, out var block) || !block.IsSolid)
                        continue;

                    var bbox = new Box3d(loc, loc + Vec3i.One);
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
        position += velocity * tmin * Vec3i.Abs(nmin);
        velocity *= Vec3i.One - Vec3i.Abs(nmin);
        collisionNormal = Vec3i.Clamp(collisionNormal + nmin, -Vec3i.One, Vec3i.One);
    }

    private static bool Collide(Box3d moving, Vec3d vel, Box3d solid, out double time, out Vec3i normal)
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
