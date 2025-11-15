namespace Crafthoe.Player.Frontend;

[Player]
public class PlayerFrontend(
    RootMouse mouse,
    RootKeyboard keyboard,
    PlayerCamera camera,
    PlayerEnt ent,
    PlayerMovement movement,
    PlayerTeleporter teleporter,
    PlayerFov fov,
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
    }

    public void Input()
    {
        movement.Input();
        construction.Input();
        camera.Rotate(-mouse.Delta / 300);
        camera.PreventBackFlipsAndFrontFlips();

        for (int i = 0; i < 9; i++)
        {
            var key = Keys.D1 + i;
            if (keyboard.IsKeyPressed(key))
                ent.Ent.HotBarIndex() = i;
        }

        ref var index = ref ent.Ent.HotBarIndex();

        if (mouse.Wheel.Y < 0)
            index++;
        else if (mouse.Wheel.Y > 0)
            index--;

        if (index < 0)
            index = HotBarSlots.Count - 1;
        if (index >= HotBarSlots.Count)
            index = 0;

        teleporter.Update();
    }
}
