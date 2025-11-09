namespace Crafthoe.Menus.Singleplayer;

[Player]
public class PlayerSingleplayerState(
    WorldTick tick,
    DimensionBackend dimensionBackend,
    DimensionRigidBag rigidBag,
    PlayerMetrics playerMetrics,
    PlayerEnt ent,
    PlayerFrontend player,
    PlayerCommonState commonState,
    PlayerSingleplayerUnloadWorldAction singleplayerUnloadWorldAction) : State
{
    public override void Load()
    {
        ent.Ent.HitBox() = new Box3d((-0.3, -0.3, -1.62), (0.3, 0.3, 0.18));
        ent.Ent.Position() = (15, 0, 120);
        ent.Ent.IsFlying() = true;
        ent.Ent.CanMove() = true;
        ent.Ent.CanFly() = true;
        ent.Ent.CanJump() = true;
        ent.Ent.CanSprint() = true;
        rigidBag.Add(ent.Ent);
        commonState.Load();
    }

    public override void Unload()
    {
        commonState.Unload();
        singleplayerUnloadWorldAction.Run();
    }

    public override void Update(double time)
    {
        commonState.Update(time);

        if (!commonState.Paused)
        {
            int ticks = tick.Update(time);
            while (ticks > 0)
            {
                if (!commonState.Inv)
                    player.Tick();
                else player.NoTick();

                    playerMetrics.TickMetric.Start();
                dimensionBackend.Tick();
                playerMetrics.TickMetric.End();

                ticks--;
            }

            player.Update(time);
            if (!commonState.Inv)
                player.Input();
        }
    }

    public override void Render()
    {
        dimensionBackend.Frame();
        commonState.Render();
    }

    public override void Draw()
    {
        commonState.Draw();
    }
}
