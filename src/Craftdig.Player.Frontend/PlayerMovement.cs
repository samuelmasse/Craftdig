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
        var movement = ent.Movement;

        if (doublePress.IsDoublePressed(Keys.W))
            movement.Sprint = MovementAction.Start;

        if (doublePress.IsDoublePressed(Keys.Space))
            movement.Fly = MovementAction.Toggle;

        ent.Movement = movement;
    }

    public void Tick()
    {
        var movement = ent.Movement;

        if (controls.CameraUp.Run())
            movement.FlyUp = true;
        if (controls.CameraDown.Run())
            movement.FlyDown = true;
        if (controls.CameraJump.Run())
            movement.Jump = true;

        Vec3d mov = default;
        if (controls.CameraFront.Run())
        {
            if (controls.CameraFast.Run())
                movement.Sprint = MovementAction.Start;

            mov += camera.Front;
        }
        else movement.Sprint = MovementAction.Stop;

        if (controls.CameraLeft.Run())
            mov -= camera.Right;
        if (controls.CameraBack.Run())
            mov -= camera.Front;
        if (controls.CameraRight.Run())
            mov += camera.Right;

        movement.Vector += (Vec3)mov.Swizzle();
        movement.LookAt = ent.LookAt;

        ent.Movement = movement;
    }

    public void NoTick()
    {
        var movement = ent.Movement;
        movement.Sprint = MovementAction.Stop;
        movement.LookAt = ent.LookAt;
        ent.Movement = movement;
    }
}
