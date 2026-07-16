namespace Craftdig.Dimension;

[Dimension]
public class DimensionMovement(DimensionPlayerBag bag)
{
    public void Tick()
    {
        foreach (var ent in bag.Ents)
        {
            if (!ent.IsRemote)
                Tick(ent);
        }
    }

    private void Tick(EntMutIdx ent)
    {
        Move(ent);
        Collide(ent);
        ent.Movement = default;
    }

    private void Collide(EntMutIdx ent)
    {
        if (ent.CollisionNormal.Z == 1)
            ent.IsFlying = false;

        if (ent.CollisionNormal.X != 0 || ent.CollisionNormal.Y != 0)
            ent.IsSprinting = false;
    }

    private void Move(EntMutIdx ent)
    {
        Fly(ent);
        Sprint(ent);
        Jump(ent);
        Vector(ent);
    }

    private void Fly(EntMutIdx ent)
    {
        if (!ent.CanFly)
        {
            ent.IsFlying = false;
            return;
        }

        bool prev = ent.IsFlying;

        switch (ent.Movement.Fly)
        {
            case MovementAction.Start:
                ent.IsFlying = true;
                break;
            case MovementAction.Stop:
                ent.IsFlying = false;
                break;
            case MovementAction.Toggle:
                ent.IsFlying = !ent.IsFlying;
                break;
            default:
                break;
        }

        if (prev && !ent.IsFlying)
            ent.IsSprinting = false;

        if (ent.IsFlying)
        {
            float speed = 0.15f;

            if (ent.Movement.FlyUp)
                ent.Velocity += (0, 0, speed);
            if (ent.Movement.FlyDown)
                ent.Velocity -= (0, 0, speed);
        }
    }

    private void Sprint(EntMutIdx ent)
    {
        if (!ent.CanSprint)
        {
            ent.IsSprinting = false;
            return;
        }

        switch (ent.Movement.Sprint)
        {
            case MovementAction.Start:
                ent.IsSprinting = true;
                break;
            case MovementAction.Stop:
                ent.IsSprinting = false;
                break;
            case MovementAction.Toggle:
                ent.IsSprinting = !ent.IsSprinting;
                break;
            default:
                break;
        }
    }

    private void Jump(EntMutIdx ent)
    {
        if (!ent.CanJump)
            return;

        if (ent.Movement.Jump && ent.CollisionNormal.Z == 1)
            ent.Velocity = ent.Velocity with { Z = 0.42 };
    }

    private void Vector(EntMutIdx ent)
    {
        if (!ent.CanMove)
            return;

        if (!ent.CanMoveVertically)
            ent.Movement = ent.Movement with { Vector = ent.Movement.Vector with { Z = 0 } };

        float speed = ent.IsFlying ? 0.05f : 0.1f;
        if (ent.IsSprinting)
            speed = ent.IsFlying ? 0.1f : 0.13f;

        var vec = ent.Movement.Vector;
        if (vec != default)
            vec = vec.Normalized;
        ent.Velocity += vec * speed;
    }
}
