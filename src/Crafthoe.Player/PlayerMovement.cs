namespace Crafthoe.Player;

[Player]
public class PlayerMovement(PlayerCamera camera, PlayerControls controls, PlayerEnt ent)
{
    private bool sprinting;

    public void Tick()
    {
        ref var vel = ref ent.Ent.Velocity();
        vel = vel.Swizzle();

        float speed = ent.Ent.IsFlying() ? 0.05f : 0.1f;

        if (ent.Ent.IsFlying())
        {
            if (controls.CameraUp.Run())
                vel.Y += speed * 3;
            if (controls.CameraDown.Run())
                vel.Y -= speed * 3;
        }
        else
        {
            if (controls.CameraJump.Run() && ent.Ent.CollisionNormal().Z == 1)
                vel.Y = 0.42;
        }

        Vector3 mov = default;
        if (controls.CameraFront.Run())
        {
            if (controls.CameraFast.Run())
                sprinting = true;

            mov += camera.Front;
        }
        else sprinting = false;

        if (ent.Ent.CollisionNormal().X != 0 || ent.Ent.CollisionNormal().Y != 0)
            sprinting = false;

        if (controls.CameraLeft.Run())
            mov -= camera.Right;
        if (controls.CameraBack.Run())
            mov -= camera.Front;
        if (controls.CameraRight.Run())
            mov += camera.Right;

        if (sprinting)
            speed = ent.Ent.IsFlying() ? 0.1f : 0.13f;

        mov.NormalizeFast();
        vel += mov * speed;
        vel = vel.Swizzle();
    }
}
