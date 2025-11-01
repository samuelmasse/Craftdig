namespace Crafthoe.Player.Frontend;

[Player]
public class PlayerMovement(
    AppDoublePress doublePress,
    PlayerPerspective perspective,
    PlayerCamera camera,
    PlayerControls controls,
    PlayerEnt ent)
{
    public void Update(double delta)
    {
        if (doublePress.IsDoublePressed(Keys.W))
            ent.Ent.Movement().Sprint = MovementAction.Start;

        if (doublePress.IsDoublePressed(Keys.Space))
            ent.Ent.Movement().Fly = MovementAction.Toggle;

        float fov = 70;
        if (ent.Ent.IsFlying())
            fov += 10;
        if (ent.Ent.IsSprinting())
            fov += 10;

        perspective.Fov = (float)MathHelper.Lerp(perspective.Fov, fov, delta * 10);
    }

    public void Tick()
    {
        if (controls.CameraUp.Run())
            ent.Ent.Movement().FlyUp = true;
        if (controls.CameraDown.Run())
            ent.Ent.Movement().FlyDown = true;
        if (controls.CameraJump.Run())
            ent.Ent.Movement().Jump = true;

        Vector3d mov = default;
        if (controls.CameraFront.Run())
        {
            if (controls.CameraFast.Run())
                ent.Ent.Movement().Sprint = MovementAction.Start;

            mov += camera.Front;
        }
        else ent.Ent.Movement().Sprint = MovementAction.Stop;

        if (controls.CameraLeft.Run())
            mov -= camera.Right;
        if (controls.CameraBack.Run())
            mov -= camera.Front;
        if (controls.CameraRight.Run())
            mov += camera.Right;

        ent.Ent.Movement().Vector += (Vector3)mov.Swizzle();
    }
}
