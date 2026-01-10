namespace Craftdig.Player.Frontend;

[Player]
public class PlayerMovement(
    AppDoublePress doublePress,
    PlayerCamera camera,
    PlayerControls controls,
    PlayerEnt ent)
{
    public void Input()
    {
        if (doublePress.IsDoublePressed(Keys.W))
            ent.Ent.Movement().Sprint = MovementAction.Start;

        if (doublePress.IsDoublePressed(Keys.Space))
            ent.Ent.Movement().Fly = MovementAction.Toggle;
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
        ent.Ent.Movement().LookAt = camera.LookAt.Swizzle();
    }

    public void NoTick()
    {
        ent.Ent.Movement().Sprint = MovementAction.Stop;
    }
}
