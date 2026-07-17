namespace Craftdig.Player.Frontend;

[Player]
public class PlayerFrontend(
    RootMouse mouse,
    RootKeyboard keyboard,
    PlayerCamera camera,
    PlayerEnt ent,
    PlayerMovement movement,
    PlayerInventoryActions inventoryActions,
    PlayerTeleporter teleporter,
    PlayerFov fov,
    PlayerCrouch crouch,
    PlayerConstruction construction)
{
    public void Tick()
    {
        movement.Tick();
        construction.Tick();
    }

    public void NoTick()
    {
        movement.NoTick();
    }

    public void Update(double delta)
    {
        fov.Update(delta);
        crouch.Update(delta);
    }

    public void Input()
    {
        movement.Input();
        construction.Input();
        if (mouse.Delta != default)
        {
            camera.Rotate(-mouse.Delta / 300);
            camera.PreventBackFlipsAndFrontFlips();
            camera.ComputeVectors();

            var lookAt = camera.LookAt.Swizzle();
            if (ent.LookAt != lookAt)
                ent.LookAt = lookAt;
        }

        var index = ent.HotBarIndex;

        for (int i = 0; i < 9; i++)
        {
            var key = Keys.D1 + i;
            if (keyboard.IsKeyPressed(key))
                index = i;
        }

        if (mouse.Wheel.Y < 0)
            index++;
        else if (mouse.Wheel.Y > 0)
            index--;

        if (index < 0)
            index = Slots.HotBarCount - 1;
        if (index >= Slots.HotBarCount)
            index = 0;

        if (ent.HotBarIndex != index)
        {
            ent.HotBarIndex = index;
            inventoryActions.Submit(InventoryOperation.SelectHotBar(index));
        }
        teleporter.Update();
    }
}
