namespace Crafthoe.Player;

[Player]
public class PlayerMovement(PlayerCamera camera, PlayerControls controls, PlayerEnt ent)
{
    public void Tick()
    {
        ref var vel = ref ent.Ent.Velocity();
        vel = vel.Swizzle();

        float speed = 0.05f;

        if (controls.CameraUp.Run())
            vel.Y += speed * 3;
        if (controls.CameraDown.Run())
            vel.Y -= speed * 3;

        if (controls.CameraFast.Run())
            speed *= 2f;

        if (controls.CameraFront.Run())
            vel += camera.Front * speed;
        if (controls.CameraLeft.Run())
            vel -= camera.Right * speed;
        if (controls.CameraBack.Run())
            vel -= camera.Front * speed;
        if (controls.CameraRight.Run())
            vel += camera.Right * speed;

        vel = vel.Swizzle();
    }
}
