namespace Crafthoe.Player.Frontend;

[Player]
public class PlayerFrontend(
    RootMouse mouse,
    RootKeyboard keyboard,
    DimensionAir air,
    DimensionBlocks blocks,
    PlayerCamera camera,
    PlayerEnt ent,
    PlayerSelected selected,
    PlayerMovement movement,
    PlayerTeleporter teleporter,
    PlayerFov fov)
{
    private int mainCooldown;
    private int secondaryCooldown;

    public void Tick()
    {
        movement.Tick();

        mainCooldown--;
        secondaryCooldown--;

        if (selected.Loc != null)
        {
            if (mouse.IsMainDown() && mainCooldown <= 0)
            {
                blocks.TrySet(selected.Loc.Value, air.Block);
                mainCooldown = 5;
            }

            var hand = ent.Ent.HotBarSlots()[ent.Ent.HotBarIndex()].Item;
            if (selected.Normal != null && hand.IsBuildable() && mouse.IsSecondaryDown() && secondaryCooldown <= 0)
            {
                blocks.TrySet(selected.Loc.Value + selected.Normal.Value, hand);
                secondaryCooldown = 4;
            }
        }
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
        camera.Rotate(-mouse.Delta / 300);
        camera.PreventBackFlipsAndFrontFlips();

        for (int i = 0; i < 9; i++)
        {
            var key = Keys.D1 + i;
            if (keyboard.IsKeyPressed(key))
                ent.Ent.HotBarIndex() = i;
        }

        if (!mouse.IsMainDown())
            mainCooldown = 0;

        if (!mouse.IsSecondaryDown())
            secondaryCooldown = 0;

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
